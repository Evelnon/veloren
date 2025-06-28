namespace Unity.Entities {
    public struct Entity {
        public int Index;
        public int Version;
    }

    public interface IComponentData {}

    public class EntityManager {
        private int _next = 1;
        public Entity CreateEntity() => new Entity { Index = _next++ };
        public void AddComponentData<T>(Entity entity, T data) where T : struct {}
        public bool HasComponent<T>(Entity entity) where T : struct => false;
    }
}
