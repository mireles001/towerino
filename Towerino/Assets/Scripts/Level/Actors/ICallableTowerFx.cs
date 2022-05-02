namespace Towerino
{
    // Interface used for extra VFX for towers, this give us freedom in
    // implemeting more non-heritance components to handle specific visual tasks
    public interface ICallableTowerFx
    {
        void ProjectileReady();
        void ProjectileFired();
        void ProjectileHit();
    }
}
