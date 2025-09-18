using Axis2.WPF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Axis2.WPF.Models;

namespace Axis2.WPF
{
    public class AnimationManager
    {
        private FileManager _fileManager;
        private IndexDataAnimation[] _m_DataIndex;

        public AnimationManager(FileManager fileManager)
        {
            _fileManager = fileManager;
            _m_DataIndex = new IndexDataAnimation[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT];
            for (int i = 0; i < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT; i++)
            {
                _m_DataIndex[i] = new IndexDataAnimation();
            }
        }

        // Votre m�thode LoadUOP existante reste inchang�e
        public bool LoadUOP()
        {
            // Part 1: Loading animation frames from AnimationFrameX.uop files
            for (int animationIndex = 0; animationIndex < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT; animationIndex++)
            {
                IndexDataAnimation indexAnim = _m_DataIndex[animationIndex];

                for (int groupIndex = 0; groupIndex < Constants.ANIMATION_UOP_GROUPS_COUNT; groupIndex++)
                {
                    string hashString = $"build/animationlegacyframe/{animationIndex:D6}/{groupIndex:D2}.bin";
                    ulong hash = UopFileReader.CreateHash(hashString);

                    for (int fileIndex = 1; fileIndex < Constants.MAX_ANIMATION_FRAME_UOP_FILES; fileIndex++)
                    {
                        UopFileReader? file = _fileManager.AnimationFrameUop[fileIndex];
                        if (file == null || !file.IsLoaded)
                        {
                            continue; // Skip if file not loaded
                        }

                        UopDataHeader? uopHeader = file.GetEntryByHash(hash);

                        if (uopHeader != null)
                        {
                            IndexDataAnimationGroupUOP group = indexAnim.AddUopGroup(groupIndex, new IndexDataAnimationGroupUOP());

                            if (group == null)
                                break;

                            for (int directionIndex = 0; directionIndex < 5; directionIndex++)
                            {
                                group.m_Direction[directionIndex] = new IndexDataFileInfo(file, uopHeader.Value);
                            }
                            break; // Found in one of the AnimationFrame files, move to next group
                        }
                    }
                }
            }

            // Part 2: Loading animation sequence from AnimationSequence.uop
            UopFileReader? animationSequenceFile = _fileManager.AnimationSequenceUop;
            if (animationSequenceFile == null || !animationSequenceFile.IsLoaded)
            {
                return false;
            }

            int parsedEntriesCount = 0;
            foreach (var entry in animationSequenceFile.GetAllEntries())
            {
                byte[]? entryData = animationSequenceFile.ReadData(entry.Value);
                if (entryData == null)
                {
                    continue;
                }

                try
                {
                    using (MemoryStream entryMs = new MemoryStream(entryData))
                    using (CustomBinaryReader entryReader = new CustomBinaryReader(entryMs))
                    {
                        uint animationIndex = entryReader.ReadUInt32LE();
                        entryReader.Move(8 * (1 + 1)); //unknown data
                        entryReader.Move(8 * (2 + 2)); //unknown data
                        uint replacesCount = entryReader.ReadUInt32LE();

                        for (int i = 0; i < replacesCount; i++)
                        {
                            uint uopGroupIndex = entryReader.ReadUInt32LE();
                            int framesCount = entryReader.ReadInt32LE();
                            uint mulGroupIndex = entryReader.ReadUInt32LE();
                            entryReader.Move(4); //unknown data
                            entryReader.Move(8 * (1 + 1)); //unknown data
                            entryReader.Move(8 * (2 + 2)); //unknown data

                            if (framesCount == 0 && uopGroupIndex < Constants.ANIMATION_GROUPS_COUNT && mulGroupIndex < Constants.ANIMATION_GROUPS_COUNT)
                            {
                                _m_DataIndex[animationIndex].m_UopReplaceGroupIndex[uopGroupIndex] = (byte)mulGroupIndex;
                            }
                            else if (framesCount > 0 && uopGroupIndex < Constants.ANIMATION_GROUPS_COUNT)
                            {
                                IndexDataAnimationGroupUOP? groupPtr = _m_DataIndex[animationIndex].GetUopGroup((int)uopGroupIndex, false);

                                if (groupPtr != null)
                                {
                                    groupPtr.SetFrameCount(framesCount);
                                }
                            }

                            int unknownCount1 = entryReader.ReadInt32LE();
                            const int validCount = 100;
                            bool invalidCount = (unknownCount1 > validCount);

                            while (unknownCount1 > 0 && !invalidCount)
                            {
                                unknownCount1--;
                                entryReader.Move(4); //unknown data
                                int unknownCount2 = entryReader.ReadInt32LE();
                                invalidCount = (unknownCount2 > validCount);

                                while (unknownCount2 > 0 && !invalidCount)
                                {
                                    unknownCount2--;
                                    entryReader.Move(4); //unknown data
                                    entryReader.Move(4); //unknown data

                                    int unknownCount3 = entryReader.ReadInt32LE();
                                    invalidCount = (unknownCount3 > validCount);
                                    entryReader.Move(unknownCount3 * (4 + 4)); //unknown data (maybe: something + graphic/animID)

                                    entryReader.Move(4); //unknown data
                                }
                            }

                            int unknownCount4 = entryReader.ReadInt32LE();
                            invalidCount = (unknownCount4 > validCount);
                            entryReader.Move(unknownCount4 * 4); //unknown data

                            if (invalidCount)
                                break;
                        }
                        parsedEntriesCount++;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return true;
        }

        // NOUVELLE M�THODE: D�couvre les actions disponibles pour une animation
        public List<int> GetAvailableActions(uint animationIndex)
        {
            Logger.Log($"DEBUG: [AnimationManager] GetAvailableActions called for animation {animationIndex}");

            var availableActions = new HashSet<int>();

            if (animationIndex >= _m_DataIndex.Length)
            {
                Logger.Log($"WARNING: [AnimationManager] animationIndex {animationIndex} is out of bounds");
                return new List<int>();
            }

            IndexDataAnimation indexAnim = _m_DataIndex[animationIndex];
            if (indexAnim == null)
            {
                Logger.Log($"WARNING: [AnimationManager] indexAnim is null for animation {animationIndex}");
                return new List<int>();
            }

            // Parcourir tous les groupes possibles (0 � Constants.ANIMATION_UOP_GROUPS_COUNT-1)
            for (int groupIndex = 0; groupIndex < Constants.ANIMATION_UOP_GROUPS_COUNT; groupIndex++)
            {
                IndexDataAnimationGroupUOP? group = indexAnim.GetUopGroup(groupIndex, false);
                if (group != null)
                {
                    // L'action est dérivée du groupIndex. Un groupe d'action commence tous les 5 groupIndex.
                    if (groupIndex % 5 == 0)
                    {
                        int action = groupIndex / 5;
                        availableActions.Add(action);
                    }
                }
            }

            var result = availableActions.OrderBy(x => x).ToList();
            Logger.Log($"DEBUG: [AnimationManager] Available actions for animation {animationIndex}: [{string.Join(", ", result)}]");
            return result;
        }

        // NOUVELLE M�THODE: Obtient la premi�re action disponible
        public int GetFirstAvailableAction(uint animationIndex)
        {
            var actions = GetAvailableActions(animationIndex);
            int firstAction = actions.Count > 0 ? actions[0] : -1;
            Logger.Log($"DEBUG: [AnimationManager] First available action for animation {animationIndex}: {firstAction}");
            return firstAction;
        }

        // Votre m�thode GetAnimationFrameData existante reste inchang�e
        public IndexDataFileInfo? GetAnimationFrameData(uint animationIndex, int groupIndex, int directionIndex)
        {
            if (animationIndex >= _m_DataIndex.Length) return null;

            IndexDataAnimation indexAnim = _m_DataIndex[animationIndex];
            if (indexAnim == null) return null;

            IndexDataAnimationGroupUOP? group = indexAnim.GetUopGroup(groupIndex, false);
            if (group == null) return null;

            if (directionIndex < group.m_Direction.Length)
            {
                return group.m_Direction[directionIndex];
            }

            return null;
        }

        // Cache pour m�moriser quelle action r�elle est utilis�e pour chaque animation
        private Dictionary<uint, int> _resolvedActionCache = new Dictionary<uint, int>();

        // M�THODE MODIFI�E: GetUopFrame avec fallback intelligent et coh�rent
        public BitmapSource? GetUopFrame(uint animationIndex, int requestedAction, int direction, int frameIndex)
        {
            Logger.Log($"DEBUG: [AnimationManager] GetUopFrame called. animationIndex: {animationIndex}, requestedAction: {requestedAction}, direction: {direction}, frameIndex: {frameIndex}");

            // D�terminer quelle action utiliser r�ellement (avec cache pour la coh�rence)
            int actualAction = GetResolvedAction(animationIndex, requestedAction);
            if (actualAction < 0)
            {
                Logger.Log($"ERROR: [AnimationManager] No actions available for animation {animationIndex}");
                return null;
            }

            // Si l'action r�solue est diff�rente de celle demand�e, l'indiquer
            if (actualAction != requestedAction)
            {
                Logger.Log($"INFO: [AnimationManager] Using action {actualAction} instead of {requestedAction} for animation {animationIndex}");
            }

            // Essayer de charger la frame avec l'action r�solue
            var frame = TryGetUopFrameForAction(animationIndex, actualAction, direction, frameIndex);
            if (frame != null)
            {
                return frame;
            }

            // Si la direction demand�e n'existe pas, essayer la direction 0
            if (direction != 0)
            {
                Logger.Log($"INFO: [AnimationManager] Direction {direction} not found, trying direction 0 for animation {animationIndex}, action {actualAction}");
                frame = TryGetUopFrameForAction(animationIndex, actualAction, 0, frameIndex);
                if (frame != null)
                {
                    return frame;
                }
            }

            Logger.Log($"ERROR: [AnimationManager] No animation data found for animation {animationIndex}");
            return null;
        }

        // NOUVELLE M�THODE: D�termine quelle action utiliser r�ellement (avec cache)
        private int GetResolvedAction(uint animationIndex, int requestedAction)
        {
            // Si on a d�j� r�solu cette animation, utiliser l'action mise en cache
            if (_resolvedActionCache.ContainsKey(animationIndex))
            {
                return _resolvedActionCache[animationIndex];
            }

            // V�rifier si l'action demand�e existe en testant son premier groupIndex
            int testGroupIndex = requestedAction * 5; // Premier groupIndex de cette action
            IndexDataFileInfo? fileInfo = GetAnimationFrameData(animationIndex, testGroupIndex, 0);

            if (fileInfo != null && fileInfo.File != null && fileInfo.File.IsLoaded)
            {
                // L'action demand�e existe, la mettre en cache
                _resolvedActionCache[animationIndex] = requestedAction;
                Logger.Log($"DEBUG: [AnimationManager] Action {requestedAction} found for animation {animationIndex}");
                return requestedAction;
            }

            // L'action demand�e n'existe pas, chercher la premi�re disponible
            int firstAvailableAction = GetFirstAvailableAction(animationIndex);
            if (firstAvailableAction >= 0)
            {
                _resolvedActionCache[animationIndex] = firstAvailableAction;
                Logger.Log($"DEBUG: [AnimationManager] Action {requestedAction} not found, resolved to action {firstAvailableAction} for animation {animationIndex}");
                return firstAvailableAction;
            }

            Logger.Log($"ERROR: [AnimationManager] No actions available for animation {animationIndex}");
            return -1;
        }

        // NOUVELLE M�THODE: Efface le cache pour une animation (utile si vous rechargez les donn�es)
        public void ClearResolvedActionCache(uint animationIndex)
        {
            _resolvedActionCache.Remove(animationIndex);
        }

        // NOUVELLE M�THODE: Efface tout le cache
        public void ClearAllResolvedActionCache()
        {
            _resolvedActionCache.Clear();
        }

        // NOUVELLE M�THODE PRIV�E: Essaie de charger une frame pour une action sp�cifique
        private BitmapSource? TryGetUopFrameForAction(uint animationIndex, int action, int direction, int frameIndex)
        {
            try
            {
                // CORRECTION: Utiliser le bon groupIndex pour l'action demand�e
                // Chaque action a 5 groupIndex cons�cutifs (un par direction)
                // Mais pour r�cup�rer le fichier .bin, on utilise le premier groupIndex de l'action
                int baseGroupIndex = action * 5; // Premier groupIndex de cette action
                Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: Using baseGroupIndex: {baseGroupIndex} for action {action}");

                // V�rifier que cette action existe r�ellement
                IndexDataFileInfo? fileInfo = GetAnimationFrameData(animationIndex, baseGroupIndex, 0);

                if (fileInfo == null)
                {
                    Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: fileInfo is null for animIndex {animationIndex}, baseGroupIndex {baseGroupIndex}.");
                    return null;
                }
                if (fileInfo.File == null)
                {
                    Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: fileInfo.File is null for animIndex {animationIndex}, baseGroupIndex {baseGroupIndex}.");
                    return null;
                }
                if (!fileInfo.File.IsLoaded)
                {
                    Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: fileInfo.File is not loaded for animIndex {animationIndex}, baseGroupIndex {baseGroupIndex}. FilePath: {fileInfo.File.FilePath}");
                    return null;
                }

                byte[]? binData = fileInfo.File.ReadData(fileInfo.UopHeader);
                if (binData == null)
                {
                    Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: binData is null after ReadData for animIndex {animationIndex}, baseGroupIndex {baseGroupIndex}.");
                    return null;
                }
                Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: binData length: {binData.Length} for animIndex {animationIndex}, baseGroupIndex {baseGroupIndex}.");

                // CORRECTION: Maintenant on passe la direction ET le frameIndex � LoadFromUopBin
                DecodedUopFrame? decodedFrame = AnimationDataLoader.LoadFromUopBin(binData, direction, frameIndex);
                if (decodedFrame == null)
                {
                    Logger.Log($"DEBUG: [AnimationManager] TryGetUopFrameForAction: decodedFrame is null after LoadFromUopBin for animIndex {animationIndex}, action {action}, direction {direction}.");
                    return null;
                }

                Logger.Log($"DEBUG: [AnimationManager] Successfully decoded UOP frame for animIndex {animationIndex}, action {action}, direction {direction}, frameIndex {frameIndex}.");
                return decodedFrame.Image;
            }
            catch (Exception ex)
            {
                Logger.Log($"ERROR: [AnimationManager] TryGetUopFrameForAction: Exception for animation {animationIndex}, action {action}, direction {direction}: {ex.Message}");
                return null;
            }
        }
    }
}