# Ranger ability sheet

Role: Ranged combat and movement  
Weapon: Thread of Dawn

## Passive

**Stillness**: Standing still for 0.8s empowers arrows by 35%.

## Abilities

| Ability | Resource cost | Cooldown | Effect |
| --- | ---: | ---: | --- |
| Piercing Shot | 18 | 4 seconds | A powerful arrow pierces all enemies. |
| Starfall Volley | 28 | 8 seconds | Release a fan of seven arrows. |
| Windstep | 22 | 9 seconds | Dodge backward and slow nearby foes. |

## Visual direction

Use a bow silhouette and narrow arrow trails. Piercing Shot is a single line, Starfall Volley is a seven-arrow fan, and Windstep has a backward movement trail.

## Display layout

Show the class name and passive above three equal ability slots. Each slot shows the ability name, resource cost, and cooldown. Dim unavailable abilities and show a countdown.

## Implementation checks

- Check that each ability matches its description.
- Test enough resource, insufficient resource, and active cooldown states.
- Check that the effect and availability are clear during combat.

These sheets describe the current design. Shared-build integration and balance testing are separate tasks.
