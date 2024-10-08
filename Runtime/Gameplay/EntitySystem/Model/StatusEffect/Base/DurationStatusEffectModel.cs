namespace Runtime.Gameplay.EntitySystem
{
    public abstract class DurationStatusEffectModel : StatusEffectModel
    {
        #region Members

        protected float duration;
        protected float bonusDuration;

        #endregion Members

        #region Properties

        public override bool IsOneShot => false;
        public virtual float Duration => duration + bonusDuration;

        #endregion Properties

        #region Class Methods

        public DurationStatusEffectModel(float duration, float chance = 1.0f) : base(chance)
        {
            this.duration = duration;
            bonusDuration = 0.0f;
        }

        public override void AddDuration(float addedDuration)
            => bonusDuration += addedDuration;

        #endregion Class Methods
    }
}