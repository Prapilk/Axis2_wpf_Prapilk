Bonjour à tous,
l'avis2 de https://github.com/DavideRei/Axis2/tree/master/release    ne fonctionnait pas pour une partie de l'équipe du shard sur lequel je travaille.
De plus l'axis2 avait une résolution inadapté aux besoins actuels.
J'ai donc repris le même visuel mais refait le tout en .net 9.0  et en WPF.
Il est adapté aux particularités du shard "UOResistance" mais il devrait fournir une bonne base pour tout ceux qui souhaite un outil SphereX puissant.

* Il faut correctement configurer son SETTINGS avec votre prefix et votre UO Title (en lien avec le nom de la fenêtre de votre shard.) dans le cas de notre shard on notera "Resistance" car c'est un élément du nom de la fenêtre.
* File Paths avec votre client et les liens muls
* L'onglet Profile devra être lancer plusieurs fois pour bien prendre en charge votre répertoire qui contient les scripts et bien actualiser votre "Load Resource". Le parsing est bon mais tout le reste est "basic"
* L'onglet Launcher est basic et fonctionne bien avec Orion, les informations sont stocké dans l'appdata/axis2
  
* L'onglet ITEM est classic et prend en charge les .mul avec la possibilité de creer des "CUSTOM LIST" et de "CLICK AND DROP" vos items.
* L'onglet ITEM TWEAK est spécial. Il contient:
* Un quick Color Palette que vous pouvez modifier avec le "CLICK DROIT" en ayant préalablement choisis une couleur dans "COLOR SELCTOR", vous pourrez sauver plusieurs palettes pour partager avec vos équipes de builder.
* Open Light Wizard est complètement spécifique à UORESISTANCE, donc inutilisable.
* Les attributes fonctionnent mais le reste n'est pas implémenté pour le moment.

* L'onglet SPAWN lis parfaitement les animations .uop et .mul.
* Le Place fait aussi tout et donc le INIT est inutile. Il prends également en charge le hue du body.def. Normalement vous devriez voir parfaitement l'ensemble.
* (Je suis content car j'ai voulu honoré PUNT qui malheureusement ne donne plus signe de vie et qui était un pro des .uop et des fichiers UO en général ! )
* Le SPAWN comprends donc parfaitement le parsing des scripts SphereX avec les liens body.def, mobtypes et bodyconv.
   
* L'onglet TRAVEL permet de se balader à la souris avec "click gauche" mais aussi en zoom à la molette et en click and drop.
* J'ai pas mal bossé sur le moyen de directement modifier dans le fichier .scp les Area et Room. Vous devez sélectionner l'encart "RECTANGLES" et vous pourrez modifier ou rajouter des ARFEA et ROOM directement sur la MAP !!!

* L'onglet "MISC" est assez spécifique à UORESISTANCE mais fait le "basic" et devrait être facilement modifiable. Attention pour ORIOn j'ai du faire un ID -1 par rapport à soundIDX.mul.

* PLAYER TWEAK; ACCOUNT, COMMANDS, REMINDER ne fonctionne pas !
* LOG reprends quelques LOgs qui provient directement des Logger.log

Prapilk 18/09/2025
