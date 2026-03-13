namespace SDIFrontEnd_WPF
{
    public interface IRecord<T>
    {
        bool NewRecord { get; set; }
        bool Dirty { get; set; }
        bool Deleted { get; set; } // marked for deletion
        bool ShouldSave { get; }
        T Item { get; set; }

    }
}
