# Data files

This folder holds the game's data as JSON.

## 1. What each file is
- `classes.json`: the playable classes, each with stats, weapon, three abilities, and passive
- `enemies.json`: regular enemies and bosses
- `items.json`: consumable items that the player can pick up and use
- `levels.json`: levels and the rooms inside them, naming enemies, bosses, and items by id

## 2. Every box
Every box listed is required unless the entry says otherwise.

### classes.json
Top level: an object with one key, `classes`, a list of class entries.

**Class entry**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique across the whole file, lowercase, underscores |
| display_name | text | shown to the player |
| description | text | one sentence |
| role | text | fixed list: `tank`, `ranged`, `caster` |
| stats | box | see Stats |
| weapon | box | see Weapon |
| ability_1, ability_2, ability_3 | box | see Ability. Three separate boxes, not a list |
| passive | box | see Passive |

**Stats**
| Box | Kind of value | Allowed values |
|---|---|---|
| max_health | number | hit points |
| move_speed_px_per_second | number | pixels per second |
| damage_reduction_percent | number | whole percent, 0 to 100 |

**Weapon**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique across the whole file |
| display_name | text | shown to the player |
| attack_type | text | fixed list: `melee`, `projectile` |
| damage | number | damage per hit |
| attack_cooldown_seconds | number | seconds between attacks |
| range_px | number | reach in pixels |
| projectile_speed_px_per_second | number | `0` for melee weapons |

**Ability** (ability_1, ability_2, ability_3)
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique across the whole file |
| display_name | text | shown to the player |
| description | text | one sentence |
| cooldown_seconds | number | seconds before reuse |

**Passive*
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique across the whole file |
| display_name | text | shown to the player |
| description | text | one sentence |

### enemies.json
Top level: an object with one key, `enemies`, a list of enemy entries.

**Enemy entry**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique, lowercase, underscores |
| display_name | text | shown to the player |
| type | text | fixed list: `regular`, `boss` |
| max_health | number | hit points |
| move_speed_px_per_second | number | pixels per second. `0` for bosses |
| detection_range_px | number | how close the player must be before the enemy chases. `0` for bosses |
| basic_attack | box | required when type is `regular`, absent for bosses |
| boss | box | required when type is `boss`, absent for regular enemies |

**basic_attack**
| Box | Kind of value | Allowed values |
|---|---|---|
| attack_type | text | fixed list: `melee`, `projectile` |
| damage | number | damage per hit |
| attack_range_px | number | how close it must be to attack |
| attack_cooldown_seconds | number | seconds between attacks |
| keep_distance_px | number | distance a ranged enemy tries to keep. `0` for melee |
| projectile_speed_px_per_second | number | `0` for melee |

**boss**
| Box | Kind of value | Allowed values |
|---|---|--- |
| phase_2_health_percent | number | whole percent. `50` means phase 2 starts at 50% health |
| attack_wait_seconds_phase_1 | number | seconds between attacks in phase 1 |
| attack_wait_seconds_phase_2 | number | seconds between attacks in phase 2 |
| attacks | list of boxes | exactly 3 entries, each with `id` (text, unique), `display_name` (text), and `damage` (number) |

### items.json
Top level: an object with one key, `items`, a list of item entries.

**Item entry**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique, lowercase, underscores |
| display_name | text | shown to the player |
| description | text | one sentence |
| type | text | fixed list: `consumable` |
| max_stack | number | how many fit in one slot. `1` for anything that doesn't stack |
| consumable | box | required when type is `consumable` |

**consumableas**
| Box | Kind of value | Allowed values |
|---|---|---|
| effect_type | text | fixed list: `heal` |
| amount | number | flat health points restored (not a percent) |

New item types are added when a card needs one: add a value to `type` and a
matching box.

### levels.json
Top level: an object with one key, `levels`, a list of level entries.

**Level entry**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique, lowercase, underscores |
| level_number | number | 1 to 4. Decides the order of levels |
| display_name | text | shown to the player |
| room_width_tiles | number | `20` |
| room_height_tiles | number | `16` |
| tile_size_px | number | `32` |
| start_room_id | text | an id from this level's `rooms` |
| rooms | list of boxes | see Room entry |

**Room Entry**
| Box | Kind of value | Allowed values |
|---|---|---|
| id | text | unique across the whole file |
| type | text | fixed list: `combat`, `boss`, `rest`, `treasure` |
| next_room_ids | list of text | room ids a door leads to. Empty for the last room of a level |
| combat | box | required when type is `combat`, absent otherwise |
| boss | box | required when type is `boss`, absent otherwise |
| treasure | box | required when type is `treasure`, absent otherwise |

Rest rooms have no extra box. The type says everything the game needs.

**combat**
| Box | Kind of value | Allowed values |
|---|---|---|
| enemy_slot_1, enemy_slot_2, enemy_slot_3 | box | each holds `enemy_id` (text, an id from `enemies.json`) |

**boss**
| Box | Kind of value | Allowed values |
|---|---|---|
| boss_id | text | an id from `enemies.json` whose type is `boss` |

**treasure**
| Box | Kind of value | Allowed values |
|---|---|---|
| item_id | text | an id from `items.json` |

## 3. Rules for a loader

1. **Kind boxes.** The box that matches `type` is required, and the other
   kind-specific boxes must be absent. This applies to enemies (`basic_attack`
   vs. `boss`), items (`consumable`), and rooms (`combat`, `boss`, `treasure`).
2. **Boss attacks.** A boss has exactly 3 entries in `attacks`.
3. **Unique ids.** Ids must be unique across their whole file.
4. **Ids must exist.**
- `start_room_id` must be a room in the same level.
- Every id in `next_room_ids` must be a room in the same level.
- Every `enemy_id` must exist in `enemies.json`.
- Every `boss_id` must exist in `enemies.json` and have type `boss`.
- Every `item_id` must exist in `items.json`.
5. **Level size.** A real level has 8 rooms. The sample has 4.
6. **Level order.** Levels run in `level_number` order. The last room of a
   level has an empty `next_room_ids`, and play continues to the level with
   the next `level_number`.

## 4. Conventions
-   Percent fields use whole numbers: `30` means 30%.
-   Units are in the box names (`_seconds`, `_px`, `_px_per_second`, `_percent`).
-   Every number in these files is a placeholder. Balance gets tuned later.

## 5. Choices on purpose

- **Classes has the Guardian only.** The Ranger and Arcanist will be added.
- **Weapons, skills, and passives are embedded** in each class, because they
  are exclusive to that class. There is no separate catalog for them.
- **Left out until a card needs them:** ability effect numbers (stun length,
  buff duration), boss attack types, and item types beyond `consumable`.
- **Bosses are stationary.** Their `move_speed_px_per_second` and
  `detection_range_px` are `0`.
- **levels.json covers treasure rooms only,** not every chest in the game.
- **The sample level has 4 rooms.** A real level has 8.
- **Quick slots hold consumables, and `amount` is flat health.**
- **Room size is stored once per level,** not per room, because every room is
  the same size.
- **Passives vs. stats.** When a passive's effect is already a stat (the
  Guardian's Thick Skin and `damage_reduction_percent`), the stat is the single
  source of truth. Apply it once.
- **Class speed vs. Player.cs.** `move_speed_px_per_second` is the value a
  future class-selection card should apply to the player. It is not a second
  copy of the default `MoveSpeed` in Player.cs. Dodge speed, duration, and
  cooldown are shared by every class and stay in Player.cs.