# Setting Up the Shared Projectile System

This new system uses a single shared projectile prefab with data-driven configuration, enabling object pooling and better performance.

## 🚀 Quick Start

### Step 1: Create the Shared Projectile Prefab

1. **Create a new GameObject** in your scene
2. **Add these components**:
   - `SpriteRenderer`
   - `Animator` (optional, for animations)
   - `Rigidbody2D`
   - `Collider2D` (CircleCollider2D recommended)
   - `SharedProjectile` script
3. **Configure the components**:
   - Set appropriate layers (projectile layer)
   - Configure physics settings
4. **Save as a prefab** in `Assets/Resources/Prefabs/Abilities/`

### Step 2: Create ProjectileData Assets

1. **Right-click** in `Assets/Resources/Data/Abilities/`
2. **Select** `Create > Abilities > Projectile Data`
3. **Create one for each ability type**:
   - `Lightning_ProjectileData`
   - `Fireball_ProjectileData`
   - `Wave_ProjectileData`
   - `AirShot_ProjectileData`
   - `PoisonShot_ProjectileData`

### Step 3: Configure ProjectileData Assets

For each ProjectileData asset, configure:

#### Lightning_ProjectileData
- **Projectile Name**: "Lightning"
- **Description**: "Fast lightning bolt with piercing"
- **Projectile Sprite**: Lightning sprite
- **Ability Animation**: Lightning animation controller
- **Damage**: 15
- **Duration**: 2.0
- **Speed**: 15
- **Pierce**: 5
- **Knockback**: 1.0
- **Homing**: true
- **Homing Range**: 2.0

#### Fireball_ProjectileData
- **Projectile Name**: "Fireball"
- **Description**: "Explosive fire projectile"
- **Projectile Sprite**: Fireball sprite
- **Ability Animation**: Fireball animation controller
- **Damage**: 20
- **Duration**: 3.0
- **Speed**: 8
- **Pierce**: 1
- **Knockback**: 2.0
- **Homing**: false
- **Bouncy**: true
- **Max Bounces**: 2

### Step 4: Create AbilityData Assets

1. **Right-click** in `Assets/Resources/Data/Abilities/`
2. **Select** `Create > Abilities > Ability Data`
3. **Configure each ability**:

#### Wand_AbilityData (Projectile Type)
- **Icon**: Wand icon sprite
- **Ability Name**: "Wand"
- **Description**: "Fires magical projectiles"
- **Ability Type**: Wand
- **Behavior Type**: Projectile
- **Projectile Data**: Drag Lightning_ProjectileData here
- **Base Cooldown**: 1.5
- **Base Damage**: 15
- **Ability Animation**: Wand animation controller
- **Activation Sound**: Wand sound

#### Frost_Pulse_AbilityData (Area Effect Type)
- **Icon**: Frost icon sprite
- **Ability Name**: "Frost Pulse"
- **Description**: "Releases a burst of frost energy"
- **Ability Type**: Frost_Pulse
- **Behavior Type**: AreaEffect
- **Effect Prefab**: Drag your FrostPulse effect prefab here
- **Base Cooldown**: 6.0
- **Base Damage**: 20
- **Ability Animation**: Frost pulse animation controller
- **Activation Sound**: Frost pulse sound

#### Dash_AbilityData (Movement Type)
- **Icon**: Dash icon sprite
- **Ability Name**: "Dash"
- **Description**: "Quickly dash in movement direction"
- **Ability Type**: Dash
- **Behavior Type**: Movement
- **Base Cooldown**: 3.0
- **Base Damage**: 18
- **Ability Animation**: Dash animation controller
- **Activation Sound**: Dash sound

#### Melee_AbilityData (Melee Type)
- **Icon**: Sword icon sprite
- **Ability Name**: "Melee Attack"
- **Description**: "Powerful melee attack"
- **Ability Type**: Melee
- **Behavior Type**: Melee
- **Effect Prefab**: Drag your Melee effect prefab here
- **Base Cooldown**: 0.5
- **Base Damage**: 25
- **Ability Animation**: Melee animation controller
- **Activation Sound**: Melee sound

### Step 5: Set Up the Global Projectile Manager

1. **Create a new GameObject** in your scene (name it "ProjectileManager")
2. **Add the `GlobalProjectileManager` component**
3. **Assign the shared projectile prefab** to `sharedProjectilePrefab`
4. **Add the `ProjectilePool` component** to the same GameObject
5. **Assign the shared projectile prefab** to the pool's `sharedProjectilePrefab`
6. **Configure pool settings**:
   - Initial Pool Size: 50
   - Max Pool Size: 200

### Step 6: Update AbilityData Assets

1. **Select your GlobalProjectileManager**
2. **Click "Update All Ability Data Assets"**
3. **This automatically assigns the shared prefab reference**

### Step 7: Test the System

1. **Play the game**
2. **Check the console** for any errors
3. **Use the pool management buttons** to monitor performance

## 🔧 Key Benefits

- **Object Pooling**: No more instantiation/destruction overhead
- **Memory Efficiency**: Single prefab instead of many
- **Easy Balancing**: All stats in ProjectileData assets
- **Performance**: Better frame rates with many projectiles
- **Flexibility**: Easy to create new projectile types

## 🎯 How It Works

1. **ProjectileData**: Contains all configuration (damage, speed, behavior, etc.)
2. **SharedProjectile**: Single prefab that reads from ProjectileData
3. **ProjectilePool**: Manages object pooling for efficiency
4. **GlobalProjectileManager**: Provides global access to the pool
5. **AbilityData**: References ProjectileData instead of individual prefabs
6. **SharedProjectileAbility**: Runtime component that spawns projectiles from the pool

## 🔄 System Flow

1. **Player activates ability** → `SharedProjectileAbility.Activated()` is called
2. **Ability spawns projectile** → Uses `GlobalProjectileManager.GetProjectile()`
3. **Projectile is configured** → `SharedProjectile.ConfigureProjectile()` applies ProjectileData
4. **Projectile returns to pool** → Automatically when destroyed or expired

## 🚨 Troubleshooting

### "Shared projectile prefab is not assigned" Error
- Make sure the GlobalProjectileManager has a prefab assigned
- Check that the prefab contains the SharedProjectile component

### "Projectile pool is not assigned" Error
- Ensure the ProjectilePool component is on the same GameObject
- Check that the pool has the shared prefab assigned

### Projectiles not spawning
- Verify ProjectileData is assigned to AbilityData
- Check that the GlobalProjectileManager is initialized
- Use "Validate Configuration" button to check for issues

### Performance issues
- Increase the pool size in ProjectilePool settings
- Use "Expand Projectile Pool" button during runtime
- Monitor pool statistics with GetPoolStats()

## 🔮 Advanced Features

- **Custom Behaviors**: Add custom scripts via `customBehaviorType`
- **Homing Projectiles**: Enable homing with detection range
- **Bouncy Projectiles**: Bounce off walls with energy loss
- **Particle Effects**: Spawn and hit effects
- **Audio**: Spawn and hit sounds
- **Materials**: Custom shader effects

