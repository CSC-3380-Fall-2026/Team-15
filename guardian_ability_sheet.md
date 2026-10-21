# Guardian ability sheet

Role: Close-range defense  
Weapon: Oathkeeper

## Passive

**Steadfast**: Take 20% less damage. Melee hits restore 2 resolve.

## Abilities

| Ability | Resource cost | Cooldown | Effect |
| --- | ---: | ---: | --- |
| Shield Bash | 16 | 5 seconds | Strike a wide arc. Stun nearby foes. |
| Bulwark | 24 | 12 seconds | Become invulnerable for 3 seconds. |
| Rally | 28 | 14 seconds | Restore 45 health. Empower attacks for 6 seconds. |

## Visual direction

Use a broad shield silhouette and a heavy stance. Shield Bash uses a short arc, Bulwark uses a bright shield outline, and Rally uses a recovery glow.

## Display layout

Show the class name and passive above three equal ability slots. Each slot shows the ability name, resource cost, and cooldown. Dim unavailable abilities and show a countdown.

## Implementation checks

- Check that each ability matches its description.
- Test enough resource, insufficient resource, and active cooldown states.
- Check that the effect and availability are clear during combat.

These sheets describe the current design. Shared-build integration and balance testing are separate tasks.
