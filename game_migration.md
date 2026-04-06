# Stargate FPS — Description complète du projet (référence migration Unity)

## Vue d'ensemble

FPS solo, tranche verticale de gameplay, univers Stargate SG-1.  
**Contrainte principale** : aucun asset externe (modèles 3D, sons, textures) — tout est généré procéduralement en code.

**Boucle de jeu** : Le joueur apparaît dans une base militaire de style Goa'uld (ambiance Abydos, désert + métal doré). Il doit éliminer tous les Jaffa ennemis, puis traverser la Porte des Étoiles pour gagner. Si sa vie tombe à zéro → Game Over, touche Entrée pour recommencer.

---

## Architecture générale

```
GameManager (Singleton)
├── Signaux/Events : health_changed, ammo_changed, weapon_changed,
│                   enemy_killed, game_over, victory
├── État global : enemies_remaining (int), game_ended (bool)
└── Méthodes : register_enemy(), notify_enemy_killed(),
               player_died(), player_won(), restart()

MainLevel (Scène racine)
├── LevelBuilder       — géométrie procédurale du niveau (sol, murs, couvertures, déco)
├── Stargate           — condition de victoire + portal animé
├── Player             — contrôleur FPS
├── EnemyJaffa × N     — ennemis IA (machine à états)
└── HUD (overlay 2D)   — vie, munitions, arme active, compteur ennemis, messages
```

**Important** : Le HUD est en overlay 2D plein écran mais **ne doit pas intercepter les inputs souris**. En Unity, désactiver le Raycast Target sur tous les éléments UI qui n'ont pas besoin de recevoir de clics.

---

## Niveau (LevelBuilder)

Géométrie entièrement procédurale. En Unity : instancier des `GameObject` avec `MeshFilter` + `MeshRenderer` + `BoxCollider`/`MeshCollider` au runtime, ou utiliser ProBuilder.

### Palette de couleurs

| Élément | Couleur hex |
|---|---|
| Sol sableux | `#C2996B` |
| Murs pierre/grès | `#856B47` |
| Structures métalliques | `#474751` |
| Détails Goa'uld dorés | `#B38C1A` |
| Caisses | `#665233` |
| Bandes lumineuses vertes (consoles) | `#33CC66` (émissif) |

### Plan du niveau (52 × 64 unités)

**Sol** : boîte 52×1×64, centrée en (0, -0.5, 0).

**Murs extérieurs** (hauteur 5u, épaisseur 1u) :
- Nord (z = -32) : deux segments laissant une ouverture centrale pour la Porte
- Sud (z = +32) : deux segments laissant une ouverture pour le spawn joueur
- Est (x = +26) et Ouest (x = -26) : murs pleins

**Disposition intérieure** :
- Mur de séparation central (couloir central libre)
- Couloirs latéraux est (x = +22) et ouest (x = -22)
- Salle de commandement autour de la Porte : murs métalliques à z = -18, -24 et x = ±9
- 4 piliers métalliques aux positions (±8, 0, +8) et (±8, 0, -4), hauteur 5u, avec capitel doré en haut

**Couvertures** :
- Zone spawn (z ≈ 22) : deux groupes de caisses empilées (1 + 1 dessus) en x = ±4
- Zone centrale : barricades de caisses en (±3, 0, 10) et barricade métallique en (0, 0, -2)
- Devant la Porte : deux consoles métalliques en (±4, 0, -12), avec bande lumineuse verte émissive

**Décorations** :
- Sarcophage doré : boîte 2.5×1.2×1.0 en (-18, 0.6, -5)
- Urnes cylindriques dorées (r=0.3, h=1.8) en x = ±15, z = 18
- Rampe d'accès vers la zone Porte : boîte 10×0.6×5 en (0, 0, -16), inclinée à -7° sur l'axe X

---

## Joueur (Player)

**Hiérarchie** :
```
CharacterController (racine)          ← Unity: CharacterController + Rigidbody kinematic
└── Head (Transform enfant)           ← rotation verticale caméra uniquement
    └── Camera (enfant de Head)
        └── WeaponHolder (Transform)  ← contient les armes, une seule visible à la fois
```

### Paramètres

| Propriété | Valeur |
|---|---|
| Vie max | 100 |
| Vitesse marche | 7.0 u/s |
| Vitesse sprint | 12.0 u/s |
| Vitesse saut | 5.0 u/s (vertical initial) |
| Gravité | 9.8 u/s² |
| Sensibilité souris | 0.002 rad/pixel |
| Clamp rotation verticale | ±90° (±PI/2) |

### Contrôles

| Touche | Action |
|---|---|
| WASD / Flèches | Déplacement |
| Shift | Sprint |
| Espace | Saut |
| Clic gauche | Tir (1er clic = capturer la souris si pas déjà fait) |
| 1 | Sélectionner arme 1 (P90) |
| 2 | Sélectionner arme 2 (Zat'nik'tel) |
| R | Recharger |
| Échap | Libérer la souris |
| Entrée | Recommencer (uniquement si game over) |

### Logique de tir (importante)

Deux booléens :
- `shooting` : vrai si le bouton gauche est actuellement enfoncé
- `shoot_just_pressed` : vrai uniquement sur le frame du clic (front montant), remis à false chaque frame

Dans `Update()` : si `shooting && (arme.isAutomatic || shoot_just_pressed)` → appeler `weapon.TryShoot()`.

**Raison** : gestion unifiée automatique/semi-automatique sans timer additionnel.

### Réception des dégâts

`TakeDamage(float amount)` → soustraire à la vie → émettre `health_changed` → si vie ≤ 0 → `GameManager.PlayerDied()`.

---

## Système d'armes

### WeaponBase (classe parente)

**Composants nécessaires** :
- `RayCast` depuis la caméra (longueur ~100u)
- `Timer` cadence de tir (`ShootTimer`, one-shot)
- `Timer` rechargement (`ReloadTimer`, one-shot)
- `AudioSource` 3D positionnel (`maxDistance = 50u`)

**Propriétés** :
```
weapon_name   : string
damage        : float
fire_rate     : float   // secondes entre deux tirs
max_ammo      : int
reserve_ammo  : int
reload_time   : float
is_automatic  : bool
current_ammo  : int
can_shoot     : bool
is_reloading  : bool
```

**Méthodes** :
- `TryShoot()` : vérifie `can_shoot`, `!is_reloading`, `current_ammo > 0` → appelle `Shoot()`
- `Shoot()` : `can_shoot = false`, décrémente `current_ammo`, raycast, émet `ammo_changed`, joue son, démarre ShootTimer
- `Reload()` : si pas déjà en rechargement et réserve > 0 → `is_reloading = true`, démarre ReloadTimer
- Fin ShootTimer → `can_shoot = true`
- Fin ReloadTimer → calcule `available = min(max_ammo - current_ammo, reserve_ammo)`, transfère, émet `ammo_changed`
- `Raycast` : si collision et cible possède `TakeDamage()` → appeler `TakeDamage(damage)`
- `BuildMesh()` : virtuelle, appelée dans `Start()`
- `CreateFireSound()` : virtuelle, retourne un `AudioClip`

---

### Arme 1 : P90

| Propriété | Valeur |
|---|---|
| Dégâts | 18 |
| Cadence | 0.08s (~750 coups/min) |
| Chargeur | 50 |
| Réserve | 200 |
| Rechargement | 2.2s |
| Mode | **Automatique** |

**Visuel procédural** (coordonnées locales, vue à la première personne) :
- Corps : boîte 0.06×0.08×0.35, gris foncé (`#404047`), centré en (0, 0, -0.05)
- Chargeur : boîte 0.05×0.12×0.1, gris très foncé (`#2E2E33`), en (0, -0.08, -0.02)
- Canon : cylindre r=0.012, h=0.25, gris très foncé, rotation 90° sur X, en (0, 0.01, -0.22)

**Son généré par PCM** (44100 Hz, mono, 16-bit signé little-endian) :
- Durée : 0.07s → 3087 samples
- Par sample `i`, `t = i / 44100` :
  ```
  env   = exp(-t × 25)
  noise = random(-1, 1)
  val   = clamp((noise × 0.2 + sin(2π × 32 × t) × 0.85 + sin(2π × 18 × t) × 0.4) × env, -1, 1)
  sample_int16 = int(val × 32767)
  ```

---

### Arme 2 : Zat'nik'tel

| Propriété | Valeur |
|---|---|
| Dégâts | 40 |
| Cadence | 0.5s |
| Chargeur | 20 |
| Réserve | 60 |
| Rechargement | 1.2s |
| Mode | **Semi-automatique** |

**Modes de tir cycliques** (1 → 2 → 3 → 1, cycling à chaque tir) :
- Mode 1 : Étourdissement → appelle `ZatStun()` (3s de stun) ou 50% dégâts si pas de méthode stun
- Mode 2 : Létal → `TakeDamage(9999)`
- Mode 3 : Désintégration → `TakeDamage(9999)` (l'ennemi se désintègre visuellement — échelle → 0)

**Visuel procédural** :
- Corps doré arqué : cylindre r=0.025, h=0.22, doré (`#BF991A`, metallic=0.8), rotation 90° sur X, en (0, 0, -0.08)
- Tête : boîte 0.04×0.04×0.06, doré clair (`#D9B233`, metallic=0.9), en (0, 0.01, -0.20)

**Son généré par PCM** :
- Durée : 0.24s → 10584 samples
- Par sample `i`, `t = i / 44100`, durée `D = 0.24` :
  ```
  env     = exp(-t × 9)
  phase   = 2π × (700 × t - 260 × t² / D)     // sweep 700Hz → 180Hz
  harmonic = sin(phase × 2) × 0.15
  val     = clamp((sin(phase) × 0.85 + harmonic) × env, -1, 1)
  ```

---

## Ennemis Jaffa

### Machine à états

```
PATROL → (joueur détecté à portée + ligne de vue) → CHASE
CHASE  → (joueur à portée d'attaque)              → ATTACK
ATTACK → (joueur trop loin)                        → CHASE
ATTACK → (pas de ligne de vue)                     → CHASE
Tout état + dégâts reçus                           → CHASE (si était PATROL)
Tout état                                          → STUNNED (si ZatStun)
STUNNED → (timer 3s écoulé)                        → CHASE
Tout état + vie ≤ 0                                → DEAD
```

### Paramètres

| Propriété | Valeur |
|---|---|
| Vie max | 80 |
| Vitesse patrouille | 2.5 u/s |
| Vitesse poursuite | 4.5 u/s |
| Portée de détection | 18u |
| Portée d'attaque | 7u |
| Dégâts projectile | 12 |
| Cooldown attaque | 2.0s |
| Gravité | 9.8 u/s² |

### Comportements

**PATROL** : route carrée de 4 waypoints générés au spawn (départ + 3 points à +4u sur X et Z). Avance vers le prochain waypoint, passe au suivant à < 1.5u. Scan le joueur chaque frame.

**CHASE** : se dirige vers le joueur à `chase_speed`. Oriente le personnage vers le joueur (`LookAt`). Passe en ATTACK si < `attack_range`.

**ATTACK** : s'arrête, regarde le joueur, vérifie la ligne de vue, si `can_attack` → tire un projectile → son staff → cooldown.

**Scan / ligne de vue** : raycast depuis la tête (y +1.4) vers le joueur (y +0.9). La cible doit être dans le groupe "player". Si le raycast touche autre chose en premier → pas de ligne de vue.

**Patrouille** : 4 points en carré (spawn, +4X, +4X+4Z, +4Z), puis boucle.

### Projectile (Staff Weapon)

- Spawné à la position bras droit de l'ennemi + 0.6u dans la direction de tir
- Vitesse : 18 u/s
- Dégâts : 12
- Durée de vie max : 4s
- Visuel : sphère émissive orange/feu (r=0.12, albedo `#FF8C0D`, emission `#FF5900`, multiplier ×4)
- Collision : sphère de rayon 0.18
  - Si touche le joueur → `TakeDamage(12)` + destroy
  - Si touche la géométrie (non-ennemi) → destroy
  - Si touche un ennemi → ignore

**Son du tir (staff)** généré par PCM :
- Durée : 0.30s, `D = 0.30`
- Par sample :
  ```
  env   = exp(-t × 12)
  phase = 2π × (220 × t - 80 × t² / D)      // sweep 220Hz → 60Hz
  body  = sin(phase) × 0.75
  crack = sin(phase × 3) × 0.2 × exp(-t × 30)
  val   = clamp((body + crack) × env, -1, 1)
  ```

### Animations procédurales

Pas de fichier d'animation — les rotations de pivots sont calculées en code dans `Update()`.

**Pivots** (Transform enfants du CharacterBody) :
| Pivot | Position locale | Rôle |
|---|---|---|
| `_pivot_left_leg` | (-0.12, 0.95, 0) | Jambe gauche |
| `_pivot_right_leg` | (0.12, 0.95, 0) | Jambe droite |
| `_pivot_left_arm` | (-0.25, 1.46, 0) | Bras gauche |
| `_pivot_right_arm` | (0.25, 1.46, 0) | Bras droit + bâton de combat |

**Walk animation** (active si PATROL ou CHASE) :
```
anim_time += delta × 5        // accumulateur
leg_angle  = sin(anim_time) × 0.45   // ≈ ±26°
pivot_left_leg.rotation.x  =  leg_angle
pivot_right_leg.rotation.x = -leg_angle
arm_swing = sin(anim_time) × 0.30
pivot_left_arm.rotation.x  = -arm_swing
pivot_right_arm.rotation.x =  arm_swing
```

**Attack animation** (déclenchée au tir) :
```
// Au moment du tir :
attack_anim = 1.0

// Dans Update() :
attack_anim = max(0, attack_anim - delta × 2.5)
raise = attack_anim × -1.2    // lève le bras droit vers l'avant
pivot_right_arm.rotation.x = arm_swing + raise
```

### Visuel procédural du Jaffa

Tous les meshes sont des primitives (BoxMesh, CylinderMesh, SphereMesh). Palette :

| Matériau | Couleur | Metallic | Roughness |
|---|---|---|---|
| Armure noire | `#1A120F` | 0.85 | 0.35 |
| Dorures Goa'uld | `#B88C14` | 0.95 | 0.15 |
| Peau | `#855C38` | 0.00 | 0.85 |
| Métal bâton | `#2E2924` | 0.80 | 0.40 |

**Hiérarchie des meshes** (positions en coordonnées locales du CharacterBody) :

*Fixés directement sur la racine :*
- Jambe gauche : cylindre r=0.10, h=0.95, en (-0.12, 0.47, 0) — armure (enfant du pivot gauche, pos relative (0, -0.48, 0))
- Jambe droite : idem symétrique (enfant du pivot droit)
- Genouillères : boîte 0.13×0.08×0.07, dorée, en (0, -0.40, 0.07) sur chaque pivot jambe
- Ceinture : boîte 0.38×0.09×0.30, dorée, en (0, 0.95, 0)
- Torse : cylindre r=0.20, h=0.44, en (0, 1.18, 0)
- Plastron : boîte 0.34×0.28×0.06, armure, en (0, 1.22, 0.13)
- Emblème : boîte 0.10×0.10×0.04, doré, en (0, 1.28, 0.17)
- Épaulières : boîte 0.15×0.08×0.22, dorée, en (0, 0, 0) sur chaque pivot bras
- Avant-bras : cylindre r=0.065, h=0.44, en (0, -0.25, 0) sur chaque pivot bras
- Cou : cylindre r=0.072, h=0.10, en (0, 1.60, 0)
- Tête : sphère r=0.155, en (0, 1.72, 0)
- Casque cobra : sphère r=0.185 armure en (0, 1.76, 0), crête boîte 0.055×0.22×0.055 dorée en (0, 2.00, -0.04), deux ailes boîtes 0.055×0.17×0.04 dorées en (±0.16, 1.91, -0.06)

*Attachés au pivot bras droit (position relative au pivot) :*
- Hampe du bâton : cylindre r=0.022, h=1.80, métal, en (0.18, -0.55, 0)
- Cellule basse : sphère r=0.060, dorée, en (0.18, 0.35, 0)
- Cellule haute : sphère r=0.055, dorée, en (0.18, 0.43, 0)
- Pointe : boîte 0.035×0.10×0.035, dorée, en (0.18, 0.52, 0)

### Mort

**Étourdi (ZatStun)** : `velocity = 0`, état STUNNED pendant 3s → retour CHASE.

**Mort normale** : tween sur `rotation.z` de 0 → 90° (0.3s) puis `scale` → 0 (0.5s), puis destroy.

**Mort par désintégration** : tween `scale` → 0.01 (0.5s), puis destroy.

---

## Porte des Étoiles (Stargate)

**Composants** :
- `Ring` : tore/anneau (`CSGTorus3D` en Godot, en Unity : mesh de tore procédural ou anneau ProBuilder), métal doré
- `Portal` : plan ou boîte très fine (10×10×0.1) au centre de l'anneau, transparent
- `Area3D` / `Trigger Collider` : détecte l'entrée du joueur

**États** :

| État | Portal albedo | Émission |
|---|---|---|
| Inactif | Bleu très sombre semi-transparent (`#1A1A26`, alpha 0.4) | Non |
| Actif | Bleu lumineux (`#3399FF`, alpha 0.85) | Oui — `#1A66E6` |

**Activation** : quand `enemies_remaining == 0` → passe en état actif.

**Rotation** : en état actif, le ring tourne en continu à 0.6 rad/s (multiplié par `delta`, pas de vitesse fixe par frame).

**Interaction joueur** :
- Si joueur entre dans la zone ET portail actif → `GameManager.PlayerWon()`
- Si joueur entre ET portail inactif → feedback visuel : scale ring de 1.0 → 1.12 en 0.1s puis retour 1.0 en 0.3s

---

## HUD (Interface 2D)

Éléments overlay (coordonnées d'écran à adapter) :

| Élément | Type | Contenu |
|---|---|---|
| `HealthBar` | Barre de progression 0-100 | Vie actuelle / max |
| `HealthLabel` | Texte | "85 / 100" |
| `AmmoLabel` | Texte | "47  \|  196" (chargeur | réserve) |
| `WeaponLabel` | Texte | Nom de l'arme active |
| `EnemiesLabel` | Texte | "Jaffa : 3" |
| `MessageLabel` | Texte centré | Messages contextuels (voir ci-dessous) |

**Messages** :
- Tous ennemis tués : `"Tous les Jaffa éliminés ! Rejoignez la Porte des Étoiles !"` (jaune)
- Game Over : `"MISSION ÉCHOUÉE\n[Entrée] Recommencer"` (rouge)
- Victoire : `"MISSION ACCOMPLIE\nVous avez traversé la Porte !"` (doré `#FFD900`)

**Connexions aux événements** :
- `health_changed(current, max)` → met à jour HealthBar et HealthLabel
- `ammo_changed(current, reserve)` → met à jour AmmoLabel
- `weapon_changed(weapon)` → met à jour WeaponLabel + AmmoLabel
- `enemy_killed(remaining)` → met à jour EnemiesLabel, si remaining == 0 → affiche message
- `game_over` → affiche message Game Over
- `victory` → affiche message Victoire

---

## Synthèse PCM — Méthode générale

Tous les sons sont générés à l'initialisation (pas au runtime du tir) et stockés dans un `AudioClip`.

**Format** : 44100 Hz, mono, 16-bit signé, little-endian (octet de poids faible en premier).

**Conversion valeur flottante → bytes** :
```
s = int(val × 32767)          // val dans [-1.0, 1.0]
byte[i*2]   = s & 0xFF        // octet bas
byte[i*2+1] = (s >> 8) & 0xFF // octet haut
```

**En Unity** : `AudioClip.Create("name", sampleCount, 1, 44100, false)` puis `clip.SetData(floatArray, 0)`. Unity utilise des floats directement (pas besoin de conversion en int16).

---

## Résumé des groupes / tags Unity

| Groupe Godot | Tag Unity |
|---|---|
| `"player"` | `"Player"` |
| `"enemy"` | `"Enemy"` |

Utilisés pour : filtrer les raycast hits, les collisions de projectiles, la ligne de vue.

---

## Résumé des événements globaux

| Événement | Paramètres | Émis par | Écouté par |
|---|---|---|---|
| `health_changed` | `(float current, float max)` | Player.TakeDamage | HUD |
| `ammo_changed` | `(int current, int reserve)` | WeaponBase.Shoot / Reload | HUD |
| `weapon_changed` | `(WeaponBase weapon)` | Player._SwitchWeapon | HUD |
| `enemy_killed` | `(int remaining)` | GameManager.NotifyEnemyKilled | HUD, Stargate |
| `game_over` | — | GameManager.PlayerDied | HUD |
| `victory` | — | GameManager.PlayerWon | HUD |

---

## Notes d'implémentation Unity

1. **Pas de cursor lock automatique** : implémenter `Cursor.lockState = CursorLockMode.Locked` au premier clic gauche si pas encore lockée.
2. **Son positionnel** : utiliser `AudioSource` avec `Spatial Blend = 1.0` (full 3D), `Max Distance = 50`.
3. **Géométrie procédurale** : `GameObject` instanciés avec `MeshFilter` + `MeshRenderer` + `BoxCollider`. Ou ProBuilder pour les formes plus complexes.
4. **Synthèse PCM** : `AudioClip.SetData(float[], 0)` avec les flottants directement, pas besoin de conversion int16.
5. **Pivots d'animation** : `GameObject` vides en tant qu'enfants, meshes enfants du pivot — rotation du pivot = animation du membre.
6. **Raycast arme** : `Physics.Raycast` depuis la caméra, `LayerMask` pour ignorer le joueur.
7. **Ligne de vue ennemi** : `Physics.Raycast` depuis la tête du Jaffa (y+1.4) vers le joueur (y+0.9), `QueryTriggerInteraction.Ignore` si les triggers interfèrent.
8. **Input système** : utiliser le nouveau Input System (InputAction) ou l'ancien Input Manager — peu importe, la logique est la même.
