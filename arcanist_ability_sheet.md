# Arcanist ability sheet

Role: Spells, area control, and recovery  
Weapon: Lantern Staff

## Passive

**Reservoir**: Regenerate 6 mana per second instead of 3.

## Abilities

| Ability | Resource cost | Cooldown | Effect |
| --- | ---: | ---: | --- |
| Ember Lance | 18 | 4 seconds | A piercing flame inflicts burn. |
| Winter Nova | 30 | 8 seconds | Damage and slow all nearby enemies. |
| Healing Light | 35 | 12 seconds | Restore 55 health and remove harmful effects. |

## Visual direction

Use a staff and lantern silhouette. Ember Lance uses a flame line, Winter Nova uses a circular frost burst, and Healing Light uses rising light.

## Display layout

Show the class name and passive above three equal ability slots. Each slot shows the ability name, resource cost, and cooldown. Dim unavailable abilities and show a countdown.

## Implementation checks

- Check that each ability matches its description.
- Test enough resource, insufficient resource, and active cooldown states.
- Check that the effect and availability are clear during combat.

These sheets describe the current design. Shared-build integration and balance testing are separate tasks.
