# Game Design Document — Stargate FPS
**Titre provisoire :** *Stargate: First Defense*
**Version :** 0.1 — Design initial
**Date :** 2026-04-06

---

## 1. Vision du jeu

Un FPS tactique en solo/coop se déroulant dans l'univers de Stargate SG-1. Le joueur incarne un membre d'une équipe SG envoyée en mission à travers la Porte des Étoiles. L'accent est mis sur l'exploration, la variété des environnements extraterrestres, et les combats qui mêlent technologie humaine et armes Goa'uld/Asgard/Anciens.

**Piliers de design :**
- **Exploration** — chaque planète est un biome unique avec ses secrets
- **Tactique** — les ennemis sont intelligents, le joueur doit adapter son approche
- **Lore fidèle** — respect de l'univers Stargate (races, technologie, factions)
- **Progression** — déverrouillage d'équipements et de technologies aliens

---

## 2. Contexte narratif

Le joueur fait partie de l'équipe SG-17, basée à Stargate Command (SGC), Cheyenne Mountain. Suite à la détection d'une activité Goa'uld inhabituelle, l'équipe est envoyée sur plusieurs planètes pour enquêter, récupérer de la technologie, et neutraliser des menaces.

**Antagonistes principaux :**
- Jaffa (soldats Goa'uld)
- Système Lords (boss de fin de mission)
- Réplicateurs (faction secondaire — niveaux avancés)
- Ori Priors (arc narratif tardif)

**Alliés potentiels :**
- Tok'ra (renseignement, gadgets)
- Asgard (technologie avancée, missions spéciales)
- Rebel Jaffa (support tactique)

---

## 3. Structure du gameplay

### 3.1 Boucle principale

```
SGC (Hub) → Sélection de mission → Porte des Étoiles → Planète → Objectifs → Exfiltration
                                                                      ↕
                                                               Exploration optionnelle
                                                               Technologie à collecter
                                                               Alliés à recruter
```

### 3.2 Structure des missions

Chaque mission se divise en **3 phases** :

| Phase | Description | Durée estimée |
|-------|-------------|---------------|
| **Reconnaissance** | Exploration de la zone d'arrivée, scan de la planète, collecte d'infos | 5–10 min |
| **Objectif principal** | Combat tactique, infiltration ou neutralisation | 15–25 min |
| **Exfiltration** | Retour à la Porte, poursuite possible | 5–10 min |

### 3.3 Types de missions

- **Combat** — assaut direct contre une base Goa'uld
- **Infiltration** — extraction d'un otage ou récupération de données sans alerter
- **Exploration** — cartographie d'une planète inconnue, découverte de site Ancien
- **Défense** — protéger la Porte d'une vague d'assaut Jaffa
- **Diplomatie armée** — escorte, négociation avec risque de combat

---

## 4. Système de combat

### 4.1 Armes

#### Armes humaines (USAF / SGC)
| Arme | Type | Notes |
|------|------|-------|
| P90 | SMG | Arme de base, haute cadence |
| M4A1 | Fusil d'assaut | Polyvalent, modulable |
| Sniper SR-25 | Précision | Missions longue portée |
| Mossberg 590 | Fusil à pompe | CQC, dégâts élevés |
| Pistolet Beretta M9 | Secondaire | Silencieux disponible |
| Grenades (frag/fumée/EMP) | Équipement | Indispensable en infiltration |

#### Armes aliens déverrouillables
| Arme | Origine | Capacité spéciale |
|------|---------|-------------------|
| Bâton de combat Jaffa | Goa'uld | Tir de plasma, rechargeable via cristal |
| Zat'nik'tel | Goa'uld | 1 tir = étourdi, 2 = mort, 3 = désintégration |
| Lance de Horus | Goa'uld | Sniper plasma |
| Pistolet Asgard | Asgard | Haute précision, munitions illimitées (rare) |
| Fusil Ori | Ori | Tir chargé dévastateur, lourd |

### 4.2 Mécanique de combat

- **Couverture dynamique** — s'accroupir / se pencher derrière les obstacles
- **Vie régénérante partielle** — la vie remonte au dernier quart de jauge, le reste nécessite des medkits
- **Fatigue de l'armure** — le bouclier de la combinaison SGC absorbe les premiers dégâts
- **Tir de précision** — zoom manuel sur toutes les armes (pas de snipers exclusifs)
- **Munitions limitées** — gestion de ressources, ramassage sur les ennemis

### 4.3 IA ennemie

| Ennemi | Comportement | Point faible |
|--------|-------------|-------------|
| Jaffa de base | Flanquement, couverture, appel de renforts | Tête (casque amovible) |
| Jaffa d'élite (Horus/Serpent) | Tactiques avancées, grenades plasma | Flancs |
| Drone Réplicateur | Essaims, grimpent les murs | EMP, balles en naquadah |
| Premier Ori | Bouclier d'énergie, invocations | Technologie ancienne, Sangraal |

---

## 5. Progression du joueur

### 5.1 Système de rang SGC

Le joueur monte en grade USAF en accomplissant des missions :
`Airman → Sergeant → Lieutenant → Captain → Major → Lieutenant Colonel`

Chaque rang débloque :
- Nouvelles missions
- Accès à l'armurerie avancée
- Capacités d'équipe supplémentaires

### 5.2 Arbre de compétences

Trois branches de spécialisation :

```
                    ┌─────────────────┐
                    │   COMMANDO      │ → Dégâts, Résistance, Assaut
                    ├─────────────────┤
POINTS DE MISSION → │   TECHNOLOGIE   │ → Hacking, Gadgets aliens, Craft
                    ├─────────────────┤
                    │   TACTICIEN     │ → Commandement équipe IA, Couverture, Furtivité
                    └─────────────────┘
```

### 5.3 Équipement modulable

- **Modules d'armes** : silencieux, chargeur étendu, lunette, lampe tactique
- **Combinaison SGC** : modules de protection, résistance aux environments aliens
- **Gadgets** : scanner biologique, interface naquadah (hacking alien), grenade de disruption

---

## 6. Hub — Stargate Command (SGC)

Entre les missions, le joueur revient au SGC, un hub semi-interactif :

| Zone | Fonction |
|------|----------|
| **Salle de briefing** | Sélection et préparation des missions, dialogues de lore |
| **Armurerie** | Acheter/modifier équipements, utiliser les technologies récupérées |
| **Infirmerie** | Récupération, dialogues avec l'équipe |
| **Lab de Daniel Jackson** | Indices de lore, déchiffrement de glyphes (puzzles optionnels) |
| **Salle des étoiles** | Carte galactique, découverte de nouvelles coordonnées |
| **Salle de la Porte** | Accès aux missions, cutscenes d'arrivée |

---

## 7. Planètes et environnements

### Exemples de biomes

| Planète | Biome | Faction | Objectif type |
|---------|-------|---------|---------------|
| **P3X-888** | Jungle dense | Unas / Goa'uld | Récupération d'artefact |
| **Abydos** | Désert / Pyramides | Jaffa Ra | Neutralisation de base |
| **Tollana** | Technologie avancée | Tollans | Diplomatie / Infiltration |
| **Othala** | Station spatiale | Asgard | Extraction d'allié |
| **Dakara** | Plaines / Ruines | Rebel Jaffa / Ori | Défense de position |

### Mécaniques environnementales

- **Atmosphère toxique** — combinaison nécessaire, filtre à durée limitée
- **Gravité réduite** — sauts amplifiés, déplacements modifiés
- **Zones ionisées** — équipements électroniques indisponibles, combat pur
- **Technologie Ancienne** — interagir avec des terminaux pour activer des avantages

---

## 8. Coopération (mode coop optionnel)

- **2–4 joueurs** en ligne
- Chaque joueur choisit une spécialisation (Commando, Tech, Tacticien, Médecin)
- Mécaniques de revive et de partage de ressources
- Missions adaptées : ennemis plus nombreux, objectifs divisés en équipe

---

## 9. Progression narrative

**Structure en 3 actes :**

| Acte | Antagoniste | Révélation |
|------|------------|------------|
| **1 — Menace Goa'uld** | Système Lord Ba'al | Découverte d'un réseau de portes secret |
| **2 — Invasion Réplicateur** | Reine Réplicateur | La technologie collectée permet une contre-attaque |
| **3 — Ascension des Ori** | Prior de l'Ori | Utilisation d'un artefact des Anciens pour repousser l'invasion |

---

## 10. Audio / Visuels

- **Musique** : orchestrale épique, inspirée de Joel Goldsmith (compositeur original SG-1)
- **Effets sonores** : armes distinctives (son caractéristique du Zat, du bâton de combat)
- **Direction artistique** : réalisme militaire pour le SGC, exotisme alien pour les planètes
- **Animations** : activation cinématique de la Porte (effet d'eau, son de transport)

---

## 11. Métriques cibles

| Métrique | Valeur cible |
|----------|-------------|
| Durée campagne solo | 12–18h |
| Nombre de missions | 20–25 |
| Nombre de planètes | 10–12 biomes uniques |
| Armes disponibles | 15–20 (humaines + aliens) |
| Rejouabilité coop | +50% durée de vie |

---

## 12. Prochaines étapes design

- [ ] Détailler les arbres de compétences (chaque branche)
- [ ] Concevoir les 5 premières missions en détail
- [ ] Définir le système économique (naquadah comme monnaie ?)
- [ ] Créer les fiches de chaque faction ennemie
- [ ] Prototyper la mécanique de la Porte (animation, son, gameplay d'arrivée)
- [ ] Définir le système de difficulté

---

*Document évolutif — itérations à venir selon les retours de conception.*
