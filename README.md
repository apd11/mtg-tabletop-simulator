# MTG Tabletop Simulator

A Magic: The Gathering virtual tabletop simulator built in Unity, inspired by [untap.in](https://untap.in). Play MTG online with real card art fetched live from the Scryfall API.

## Features

- **Real Card Art** — Fetched automatically from Scryfall API with in-session caching
- **Full Zone System** — Hand, Battlefield, Library, Graveyard, Exile, and Command Zone
- **Drag & Drop** — Freely move cards between zones and around the battlefield
- **Top-Down View** — Clean orthographic camera perspective
- **Deck Loader** — Load any deck in MTGO/Arena format
- **Card Browser** — Browse and search Library, Graveyard, and Exile
- **Life Counter** — Track Life, Poison, and Commander Damage with +/- buttons and direct input
- **Tap/Untap** — Tap cards with Spacebar, untap all with U
- **Zone Counts** — Live card counts displayed on each zone
- **Coin Flip** — Animated coin flip with C key
- **D20 Dice Roller** — Animated dice roll with R key
- **Context Menu** — Right-click cards for zone actions
- **Card Preview** — Hover cards to see a large preview on the right side
- **Double-Faced Cards** — Both faces fetched and stored automatically
- **Felt Mat** — Procedurally generated playmat with zone outlines and labels

## Controls

| Key | Action |
|-----|--------|
| D | Draw a card |
| U | Untap all |
| S | Shuffle library |
| C | Coin flip |
| R | Roll D20 |
| Space | Tap/Untap hovered card |
| Right Click | Context menu |
| ESC | Close open panels |

## Setup

### Requirements
- Unity 6.x or 2022 LTS
- 3D URP (SRP) project template
- Input System package

### Scene Hierarchy
```
Scene
├── Player1              ← ZoneManager.cs
│   ├── Hand_Anchor
│   ├── Battlefield_Anchor
│   ├── Library_Anchor
│   ├── Graveyard_Anchor
│   ├── Exile_Anchor
│   └── Command_Anchor
├── CardPrefab           ← CardObject.cs
│   ├── Front            ← CardHoverRelay, CardDrag, CardInteract, CardContextMenu
│   └── Back
├── GameBootstrap        ← TestBootstrap.cs
├── ScryfallLoader       ← ScryfallLoader.cs
├── DeckLoader           ← DeckLoader.cs
├── CardPreview          ← CardPreview.cs
├── TableMat             ← TableMat.cs
├── ZoneBrowser          ← ZoneBrowser.cs
├── ZoneCountDisplay     ← ZoneCountDisplay.cs
├── LifeCounter          ← LifeCounter.cs
├── CoinFlip             ← CoinFlip.cs
├── DiceRoller           ← DiceRoller.cs
└── GameUI               ← GameUI.cs
```

### Decklist Format
Paste any decklist in MTGO/Arena format into the TestBootstrap Decklist field:
```
4 Lightning Bolt
2 Counterspell
1 Black Lotus
4 Island
```

## Roadmap

- Token creation
- Scry
- Card counters (+1/+1, -1/-1)
- Clone cards
- Face down cards on battlefield
- Send cards to top/bottom of library
- Shuffle animations
- Zone count on mat
- Persistent card art cache
- Second player support
- Multiplayer via Mirror/Fishnet

## Built With
- [Unity](https://unity.com/)
- [Scryfall API](https://scryfall.com/docs/api) — Card data and artwork

## License
MIT
