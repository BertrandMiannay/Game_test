# Stargate: First Defense — Vertical Slice

FPS prototype dans l'univers Stargate, développé avec **Godot 4**.

## Prérequis

- [Godot 4.2+](https://godotengine.org/download) (version standard, pas Mono)

## Lancer le jeu

1. Ouvrir Godot 4
2. **Import** → sélectionner ce dossier `game/`
3. Appuyer sur **F5** (ou bouton Play) — la scène `main_level.tscn` se lance automatiquement

## Contrôles

| Action | Touche |
|--------|--------|
| Déplacement | WASD / Flèches |
| Saut | Espace |
| Sprint | Shift |
| Tirer | Clic gauche |
| Recharger | R |
| Arme 1 (P90) | 1 |
| Arme 2 (Zat) | 2 |
| Libérer la souris | Échap |
| Recommencer | Entrée |

## Objectif

**Éliminer les 6 Jaffa**, puis rejoindre la Porte des Étoiles (anneau doré au nord).

- La Porte est **inactive** (grise) tant qu'il reste des ennemis
- Elle s'**active** (bleue lumineuse) une fois tous les Jaffa éliminés

## Armes

### P90 `[touche 1]`
Mitraillette USAF standard. Haute cadence, 50 balles/chargeur.

### Zat'nik'tel `[touche 2]`
Pistolet énergétique Goa'uld. 3 modes cycliques :
- **1er tir** : étourdit l'ennemi pendant 3 secondes
- **2e tir** : tue instantanément
- **3e tir** : désintègre (supprime le corps)

## Structure du projet

```
game/
├── project.godot              ← Ouvrir avec Godot 4
├── scenes/
│   ├── main_level.tscn        ← Scène principale
│   ├── player.tscn            ← Joueur FPS
│   ├── enemies/jaffa.tscn     ← Ennemi Jaffa
│   └── ui/hud.tscn            ← Interface
└── scripts/
    ├── autoload/
    │   └── game_manager.gd    ← Singleton (signaux, état, touches)
    ├── player.gd              ← Mouvement FPS + caméra
    ├── weapon_base.gd         ← Classe de base armes
    ├── weapon_p90.gd          ← P90
    ├── weapon_zat.gd          ← Zat'nik'tel
    ├── enemy_jaffa.gd         ← IA Jaffa (patrouille/chase/attaque)
    ├── level_builder.gd       ← Génération CSG du niveau
    ├── stargate.gd            ← Porte des Étoiles (condition victoire)
    └── hud.gd                 ← Affichage HUD
```

## État actuel (v0.1)

- [x] Mouvement FPS (marche, sprint, saut)
- [x] 2 armes jouables (P90 + Zat avec modes spéciaux)
- [x] IA ennemie : patrouille, détection, poursuite, attaque
- [x] HUD : vie, munitions, compteur ennemis, messages
- [x] Niveau complet en géométrie CSG (base Goa'uld sur Abydos)
- [x] Condition victoire (Porte des Étoiles)
- [x] Condition défaite

## Prochaines étapes

- [ ] Son : tirs, musique ambiante, dialogues radio
- [ ] Effets visuels : muzzle flash, impacts, particules Zat
- [ ] IA améliorée : couverture, flanquement
- [ ] 2e niveau
- [ ] Remplacement des meshes CSG par de vrais modèles 3D
