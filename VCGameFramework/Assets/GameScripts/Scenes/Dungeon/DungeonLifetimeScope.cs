using VContainer;
using VContainer.Unity;

namespace Game.Scenes.Dungeon
{
    public class DungeonLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DungeonProgressManager>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MonsterSpawnController>(Lifetime.Scoped);
        }
    }
}
