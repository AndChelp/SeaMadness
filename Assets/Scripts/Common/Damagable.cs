namespace Scripts.Common
{
    public interface Damagable
    {
        void TakeDamage(int amount);
        void Die();
    }
}