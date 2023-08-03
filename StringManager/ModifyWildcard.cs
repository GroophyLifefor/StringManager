namespace Clee.Text;

public class ModifyWildcard
{
    private readonly StringManager _stringManager;
    
    /// <summary>
    /// Gets or sets the starting index of the modification range.
    /// </summary>
    public int StartIndex { get; internal set; }
    
    /// <summary>
    /// Gets or sets the ending index of the modification range.
    /// </summary>
    public int EndIndex { get; internal set; }
    
    /// <summary>
    /// Gets a read-only list of indexed wildcards associated with this modification.
    /// </summary>
    public List<IndexedWildcard> WildcardIndexes { get; } = new();

    /// <summary>
    /// Initializes a new instance of the ModifyWildcard class with the provided StringManager.
    /// </summary>
    /// <param name="stringManager">The StringManager used for text modification operations.</param>
    public ModifyWildcard(StringManager stringManager)
    {
        _stringManager = stringManager;
    }

    /// <summary>
    /// Gets the value associated with the specified wildcard name.
    /// </summary>
    /// <param name="name">The name of the wildcard to retrieve the value for.</param>
    /// <returns>The value associated with the specified wildcard name, or an empty string if the wildcard is not found.</returns>
    public string GetValue(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        IndexedWildcard? wildcard = WildcardIndexes.FirstOrDefault(x => x.Name == name);
        
        return (wildcard is null ? string.Empty : wildcard.Value)!;
    }

    /// <summary>
    /// Gets the indexed wildcard object with the specified name.
    /// </summary>
    /// <param name="name">The name of the indexed wildcard to retrieve.</param>
    /// <returns>The indexed wildcard object with the specified name, or null if the wildcard is not found.</returns>
    public IndexedWildcard? GetWildcard(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return WildcardIndexes.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Sets the value of the specified wildcard by name.
    /// </summary>
    /// <param name="name">The name of the wildcard to set the value for.</param>
    /// <param name="value">The value to assign to the wildcard.</param>
    public void SetValue(string name, string value)
    {
        var item = GetWildcard(name);
        
        if (item is { Value: { } }) 
            _stringManager.Replace(item.Index, item.Value.Length, value);
    }

    /// <summary>
    /// Modifies the body of the text using the provided function that takes a dictionary of wildcard names and their values.
    /// </summary>
    /// <param name="func">The function that modifies the text using wildcard values as input.</param>
    public void ModifyBody(Func<Dictionary<string, string>, string> func)
    {
        Dictionary<string, string> wildcards = new Dictionary<string, string>();
        WildcardIndexes.ForEach(wildcard => wildcards.Add(wildcard.Name, wildcard.Value));
        _stringManager.Replace(StartIndex, EndIndex - StartIndex, func(wildcards));
    }

    /// <summary>
    /// Replaces the content within the specified range with the given text.
    /// </summary>
    /// <param name="text">The text to replace the content with.</param>
    public void Replace(string text)
        => _stringManager.Replace(StartIndex, EndIndex - StartIndex, text);
}