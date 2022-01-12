namespace KaizerWaldCode.PlayerEntityInteractions
{
    public interface ISelectable
    {
        public bool IsSelected { get; }
        public bool IsPreselected { get; }

        public void SetSelected(bool enable);
        public void SetPreselected(bool enable);
    }
}