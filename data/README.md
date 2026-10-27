# Data Files

This folder holds the game's data as JSON files. Designers can change enemies, items, and levels here without touching any code.

## How to read these files

- A **box** is one piece of information, like `max_health`.
- A **list** holds several things in order, like the rooms in a level.
- An **id** is a unique name, like `skeleton_grunt`. Other files use ids to point at things instead of copying them.
- Every box is required unless a table says otherwise.
- Units are in the box names: `_seconds`, `_px` (pixels), `_tiles`, `_percent`.
- Percent boxes use whole numbers: `30` means 30%.
- Every number is a placeholder. We will balance them later.

## The four files

| File | What it holds |
|---|---|
| `classes.json` | The characters the player can pick |
| `enemies.json` | Regular enemies and bosses |
| `items.json` | Items the player can pick up and use |
| `levels.json` | Levels and the rooms inside them |

## classes.json

A list called `classes`. Each entry is one character, like the Guardian.

**Class**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| description | short description | one sentence |
| role | what the class does | `tank`, `ranged`, or `caster` |
| stats | the class numbers | see Stats |
| weapon | the class weapon | see Weapon |
| ability_1, ability_2, ability_3 | the three skills | see Ability |
| passive | the always-on skill | see Passive |

**Stats**

| Box | What it is | Allowed values |
|---|---|---|
| max_health | hit points | number |
| move_speed_px_per_second | how fast the class moves | number |
| damage_reduction_percent | how much damage is blocked | 0 to 100 |

**Weapon**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| attack_type | how it attacks | `melee` or `projectile` |
| damage | damage per hit | number |
| attack_cooldown_seconds | wait between attacks | number |
| range_px | how far it reaches | number |
| projectile_speed_px_per_second | how fast the shot flies | number, `0` for melee |

**Ability** (used for ability_1, ability_2, and ability_3)

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| description | what it does | one sentence |
| cooldown_seconds | wait before using it again | number |

**Passive**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| description | what it does | one sentence |

## enemies.json

A list called `enemies`. Each entry is a regular enemy or a boss. Our examples are `skeleton_grunt` (melee), `skeleton_archer` (ranged), and `grave_warden` (boss).

**Enemy**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| type | what kind of enemy | `regular` or `boss` |
| max_health | hit points | number |
| move_speed_px_per_second | how fast it moves | number, `0` for bosses |
| detection_range_px | how close the player must be before it chases | number, `0` for bosses |
| basic_attack | the regular attack | required for `regular`, left out for `boss` |
| boss | the boss details | required for `boss`, left out for `regular` |

**basic_attack**

| Box | What it is | Allowed values |
|---|---|---|
| attack_type | how it attacks | `melee` or `projectile` |
| damage | damage per hit | number |
| attack_range_px | how close it must be to attack | number |
| attack_cooldown_seconds | wait between attacks | number |
| keep_distance_px | distance a ranged enemy tries to keep | number, `0` for melee |
| projectile_speed_px_per_second | how fast the shot flies | number, `0` for melee |

**boss**

| Box | What it is | Allowed values |
|---|---|---|
| phase_2_health_percent | when phase 2 starts | whole percent, `50` means at 50% health |
| attack_wait_seconds_phase_1 | wait between attacks in phase 1 | number |
| attack_wait_seconds_phase_2 | wait between attacks in phase 2 | number |
| attacks | the boss attacks | a list of exactly 3, each with `id`, `display_name`, and `damage` |

Bosses do not move, so their speed and detection range are `0`.

## items.json

A list called `items`. Each entry is one item.

**Item**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| display_name | name shown to the player | text |
| description | what it does | one sentence |
| type | what kind of item | `consumable` (the only type so far) |
| max_stack | how many fit in one slot | number, `1` if it does not stack |
| consumable | the use details | required when type is `consumable` |

**consumable**

| Box | What it is | Allowed values |
|---|---|---|
| effect_type | what it does | `heal` (the only effect so far) |
| amount | health restored | flat number, not a percent |

When a card needs a new kind of item, add a value to `type` and a matching box.

## levels.json

A list called `levels`. Each level has rooms.

**Level**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | lowercase with underscores |
| level_number | the order of the level | 1 to 4 |
| display_name | name shown to the player | text |
| room_width_tiles | room width | `20` |
| room_height_tiles | room height | `16` |
| tile_size_px | size of one tile | `32` |
| start_room_id | the room the player starts in | an id from this level's rooms |
| rooms | the rooms in the level | a list of rooms |

Room size is written once per level because every room is the same size.

**Room**

| Box | What it is | Allowed values |
|---|---|---|
| id | unique name | unique in the whole file |
| type | what kind of room | `combat`, `boss`, `rest`, or `treasure` |
| next_room_ids | the rooms the door leads to | a list of room ids, empty for the last room |
| obstacles | solid blocks in the room | a list, required, can be empty |
| combat | the enemies | required for `combat` rooms, left out otherwise |
| boss | the boss | required for `boss` rooms, left out otherwise |
| treasure | the chest item | required for `treasure` rooms, left out otherwise |

Rest rooms have no extra box. The type tells the game everything it needs.

**Obstacle**

Positions count in tiles from the top-left corner of the room. The top-left tile is x 0, y 0.

| Box | What it is | Allowed values |
|---|---|---|
| x_tile | tiles from the left | 0 or higher |
| y_tile | tiles from the top | 0 or higher |
| width_tiles | how wide | 1 or higher |
| height_tiles | how tall | 1 or higher |

**combat**

| Box | What it is | Allowed values |
|---|---|---|
| enemy_slot_1, enemy_slot_2, enemy_slot_3 | the three enemies | each holds an `enemy_id` from `enemies.json` |

**boss**

| Box | What it is | Allowed values |
|---|---|---|
| boss_id | which boss | an id from `enemies.json` with type `boss` |

**treasure**

| Box | What it is | Allowed values |
|---|---|---|
| item_id | which item the chest holds | an id from `items.json` |

Example of a treasure room:

~~~json
{
  "id": "level_1_room_3",
  "type": "treasure",
  "next_room_ids": ["level_1_room_4"],
  "obstacles": [],
  "treasure": { "item_id": "health_potion" }
}
~~~

It points at `health_potion` by id. The data for the potion lives in `items.json`, so it is never copied here.

## Rules for the loader

JSON cannot check these rules on its own, so the code that loads the files has to.

1. **Match the type.** The box that matches `type` must be there, and the other type boxes must be left out. A `boss` enemy has a `boss` box and no `basic_attack`. This works the same way for items and rooms.
2. **Three boss attacks.** A boss has exactly 3 attacks.
3. **Unique ids.** No two things in the same file can share an id.
4. **Ids must exist.**
   - `start_room_id` and every id in `next_room_ids` must be a room in the same level.
   - Every `enemy_id` must be in `enemies.json`.
   - Every `boss_id` must be in `enemies.json` and have type `boss`.
   - Every `item_id` must be in `items.json`.
5. **Eight rooms.** A real level has 8 rooms. Our sample level has all 8.
6. **Level order.** Levels run in `level_number` order. When the last room of a level is done, play continues to the next level.
7. **Obstacles fit in the room.** `x_tile + width_tiles` cannot be more than `room_width_tiles`, and `y_tile + height_tiles` cannot be more than `room_height_tiles`.

## Choices we made on purpose

- **Only the Guardian is in classes.json so far.** The Ranger and Arcanist will be added.
- **Weapons, abilities, and passives live inside each class,** because each class has its own and nothing is shared.
- **Left out until a card needs them:** ability effect numbers (like stun length), boss attack types, and item types other than `consumable`.
- **Bosses are stationary.**
- **Quick slots hold consumables.**
- **Passives and stats are not counted twice.** If a passive's effect is already a stat (Thick Skin and `damage_reduction_percent`), the stat is the one source of truth.
- **Class speed vs. Player.cs.** `move_speed_px_per_second` is the speed a class should use. It is not a second copy of `MoveSpeed` in `Player.cs`. The dodge numbers are the same for every class and stay in `Player.cs`.
