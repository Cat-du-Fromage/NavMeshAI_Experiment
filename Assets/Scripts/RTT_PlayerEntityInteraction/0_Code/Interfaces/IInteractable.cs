namespace KaizerWaldCode.PlayerEntityInteractions
{
    public interface IInteractable
    {
        public bool IsSelected { get; }

        public void SetSelected(bool enable);
    }
}