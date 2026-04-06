# Stargate: First Defense — Unity Implementation

Portage Unity/C# du jeu Stargate FPS développé initialement sous Godot 4.

## Prérequis

- **Unity 2022.3 LTS** ou supérieur (2021.3+ compatible)
- Aucun package externe requis
- Aucun asset externe — tout est procédural

## Installation

1. Ouvrir Unity Hub
2. `Add project from disk` → sélectionner ce dossier `unity_implementation/`
3. Unity importera et compilera automatiquement tous les scripts C#
4. Ouvrir la scène : `Assets/Scenes/MainLevel.unity`
5. Appuyer sur **Play**

## Architecture

```
Assets/Scripts/
├── GameManager.cs        Singleton C# — events, état global
├── PlayerController.cs   CharacterController FPS (WASD, souris, sauts)
├── WeaponBase.cs         Classe de base armes (raycast, ammo, reload)
├── WeaponP90.cs          P90 automatique (18 dmg, 50 balles, 750 coups/min)
├── WeaponZat.cs          Zat'nik'tel 3 modes : stun / kill / désintégration
├── EnemyJaffa.cs         IA FSM : PATROL → CHASE → ATTACK → STUNNED → DEAD
├── Projectile.cs         Plasma orange (vitesse 18, dmg 12, 4s durée vie)
├── LevelBuilder.cs       Niveau 52×64 entièrement procédural
├── Stargate.cs           Portail victoire (activé à 0 ennemi restant)
├── HUD.cs                Interface : santé, ammo, ennemis, messages
├── SoundGenerator.cs     Audio PCM procédural (P90 / Zat / Staff)
└── SceneBootstrap.cs     Construit toute la scène au runtime depuis Awake()
```

## Contrôles

| Action | Touche |
|--------|--------|
| Avancer | W / Flèche haut |
| Reculer | S / Flèche bas |
| Gauche | A / Flèche gauche |
| Droite | D / Flèche droite |
| Sauter | Espace |
| Sprinter | Shift gauche |
| Tirer | Clic gauche |
| Recharger | R |
| Arme 1 (P90) | 1 |
| Arme 2 (Zat) | 2 |
| Libérer souris | Échap |
| Recommencer | Entrée |

## Objectif

1. Éliminer les **6 soldats Jaffa**
2. Atteindre la **Porte des étoiles** (nord du niveau) une fois tous les ennemis vaincus

## Correspondances Godot → Unity

| Godot | Unity |
|-------|-------|
| `CharacterBody3D` | `CharacterController` |
| `signal` | `event Action<T>` |
| `RayCast3D` | `Physics.Raycast()` |
| `Area3D` trigger | `BoxCollider isTrigger` |
| `CSGBox3D` | `CreatePrimitive(Cube)` |
| `Tween` | Coroutine |
| `AudioStreamWAV` | `AudioClip.Create()` PCM |
