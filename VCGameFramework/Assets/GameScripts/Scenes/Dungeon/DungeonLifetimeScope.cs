using VContainer;
using VContainer.Unity;

namespace Game.Scenes.Dungeon
{
    /// <summary>
    /// 副本场景的作用域配置
    /// </summary>
    public class DungeonLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// 注册副本相关入口点
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DungeonProgressManager>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MonsterSpawnController>(Lifetime.Scoped);
        }
    }
}
