# Sistema de Combate - Guía de Configuración

## ✅ Implementación Completada

Se ha implementado un sistema completo de combate donde las torres disparan proyectiles a los enemigos que tienen vida y mueren con efecto de desvanecimiento.

## 📁 Archivos Creados

### Scripts Nuevos
1. **`Assets/Scripts/Projectile.cs`** - Comportamiento de los proyectiles
2. **`Assets/Scripts/TowerShooter.cs`** - Sistema de disparo de las torres
3. **`Assets/Scripts/TowerStats.cs`** - ScriptableObject para estadísticas de torres

### Scripts Modificados
1. **`Assets/Scripts/EnemyWalker.cs`** - Añadido sistema de vida y muerte
2. **`Assets/Scripts/EnemySpawner.cs`** - Configuración de vida inicial
3. **`Assets/Scripts/TowerPlacementManager.cs`** - Integración del shooter

### Prefabs
1. **`Assets/Prefabs/Projectile.prefab`** - Prefab del proyectil

## 🎮 Configuración en Unity

### Paso 1: Configurar el Prefab de Proyectil

1. Abre Unity y selecciona el prefab `Assets/Prefabs/Projectile.prefab`
2. En el Inspector, asigna un sprite para el proyectil:
   - Puedes usar cualquier sprite pequeño que represente una nota musical o proyectil
   - Busca en `Assets/Sprites/` un sprite adecuado
3. Ajusta el color si lo deseas (actualmente amarillo/dorado)
4. El prefab ya tiene:
   - ✅ Componente `Projectile` configurado
   - ✅ CircleCollider2D como trigger
   - ✅ SpriteRenderer con sorting order 15

### Paso 2: Configurar el TowerPlacementManager

1. Selecciona el GameObject que tiene el `TowerPlacementManager` en tu escena
2. En el Inspector, encontrarás nuevos campos en la sección "Combat":
   - **Default Projectile Prefab**: Arrastra aquí el prefab `Projectile`
   - **Enemy Layer**: Configura la capa donde están los enemigos (si usas una específica)

### Paso 3: Configurar los Enemigos

1. En el `EnemySpawner`, ahora hay un nuevo campo:
   - **Enemy Max Health**: Define cuánta vida tendrán los enemigos (default: 100)
2. Ajusta este valor según la dificultad deseada

### Paso 4: (Opcional) Crear TowerStats Assets

Para torres con diferentes estadísticas:

1. En Unity, haz clic derecho en la carpeta Project
2. Selecciona `Create > Tower Defense > Tower Stats`
3. Nombra el asset (ej: "GuitarTower_Stats", "DrumsTower_Stats")
4. Configura las estadísticas:
   - **Tower Name**: Nombre descriptivo
   - **Damage**: Daño por proyectil (ej: 25)
   - **Fire Rate**: Tiempo entre disparos en segundos (ej: 1.0)
   - **Range**: Alcance de detección (ej: 5.0)
   - **Projectile Speed**: Velocidad del proyectil (ej: 10)
   - **Projectile Prefab**: Arrastra el prefab Projectile aquí
5. Repite para cada tipo de torre diferente

### Paso 5: Asignar Stats a Torres (Opcional)

Si creaste TowerStats assets:

1. Selecciona el `TowerPlacementManager`
2. Expande el array "Tower Stats"
3. Ajusta el tamaño al número de torres que tienes
4. Asigna cada TowerStats asset a su torre correspondiente (mismo orden que Tower Prefabs)

## ⚙️ Parámetros Configurables

### En TowerShooter (se configura automáticamente)
- **Damage**: Daño por proyectil
- **Fire Rate**: Segundos entre disparos
- **Range**: Radio de detección de enemigos
- **Projectile Speed**: Velocidad del proyectil
- **Show Range**: Ver el rango en el editor (gizmo)

### En Projectile
- **Speed**: Velocidad de movimiento
- **Damage**: Daño al impactar
- **Max Lifetime**: Tiempo antes de auto-destruirse (segundos)
- **Rotation Speed**: Velocidad de rotación visual (grados/segundo)

### En EnemyWalker
- **Max Health**: Vida máxima del enemigo
- **Fade Out Duration**: Duración del efecto de desvanecimiento al morir

## 🎯 Características Implementadas

### Sistema de Vida de Enemigos ✅
- Los enemigos tienen vida configurable
- Método `TakeDamage(float damage)` para recibir daño
- Sistema de muerte con desvanecimiento visual
- Los enemigos destruidos desaparecen gradualmente

### Sistema de Proyectiles ✅
- Proyectiles visuales que viajan hacia el objetivo
- Seguimiento del enemigo (homing)
- Detección de colisión con trigger
- Auto-destrucción al impactar o después del tiempo máximo
- Rotación visual para efecto

### Sistema de Disparo de Torres ✅
- Detección automática de enemigos en rango
- Targeting del enemigo más cercano
- Sistema de cooldown (fire rate)
- Rotación de la torre hacia el objetivo
- Compatible con múltiples enemigos

### Sistema de Stats por Torre ✅
- ScriptableObjects para configurar diferentes torres
- Cada torre puede tener:
  - Daño único
  - Velocidad de disparo única
  - Rango único
  - Velocidad de proyectil única
  - Prefab de proyectil único

## 🔧 Solución de Problemas

### Las torres no disparan
1. Verifica que el `defaultProjectilePrefab` esté asignado en TowerPlacementManager
2. Asegúrate de que los enemigos estén dentro del rango de la torre
3. Verifica que el prefab Projectile tenga el componente `Projectile`

### Los proyectiles no dañan a los enemigos
1. Verifica que el Projectile tenga un CircleCollider2D con "Is Trigger" activado
2. Asegúrate de que los enemigos tengan colliders
3. Verifica las capas de colisión en Physics2D Settings

### Los enemigos no mueren
1. Verifica que el daño del proyectil sea mayor que 0
2. Asegúrate de que la vida del enemigo esté configurada correctamente
3. Revisa la consola por errores

### Las torres no rotan
1. Verifica que el componente TowerShooter tenga asignado el `visualTransform`
2. Esto se configura automáticamente al colocar la torre

## 🎨 Mejoras Futuras Sugeridas

1. **Efectos visuales**: Partículas al impactar
2. **Efectos de sonido**: Sonidos de disparo y muerte
3. **Múltiples tipos de proyectiles**: Notas musicales diferentes
4. **Torres especiales**: Torres con habilidades únicas
5. **Sistema de puntos**: Recompensas por eliminar enemigos
6. **Barra de vida visual**: UI mostrando la vida del enemigo
7. **Efectos de área**: Torres que dañan múltiples enemigos

## 📊 Valores Recomendados

### Torre Básica
- Damage: 25
- Fire Rate: 1.0s
- Range: 5.0
- Projectile Speed: 10

### Torre Rápida
- Damage: 15
- Fire Rate: 0.5s
- Range: 4.0
- Projectile Speed: 15

### Torre Poderosa
- Damage: 50
- Fire Rate: 2.0s
- Range: 6.0
- Projectile Speed: 8

### Enemigo Débil
- Max Health: 50

### Enemigo Normal
- Max Health: 100

### Enemigo Fuerte
- Max Health: 200

---

¡El sistema está listo para usar! Solo necesitas configurar los valores en Unity según tus preferencias. 🎸🎮

