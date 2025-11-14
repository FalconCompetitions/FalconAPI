---
applyTo: '**/.cs'
---
When generating documentation for Csharp files, ensure that the comments are clear and concise. Use XML documentation comments to provide context for each function, class, or interface. Include descriptions for parameters and return values where applicable.
Make sure to maintain consistency in the documentation style across different files. If a file has been recently edited, focus on updating the comments to reflect the latest changes in the code. Avoid suggesting code that has been deleted or is no longer relevant.
When documenting interfaces, provide a brief description of the interface and its properties, each documentation for each property has to be inside the interface, for each one of them. If a property is optional, indicate this clearly in the documentation.

if the interface is:
```csharp
public interface Example {
    public string ExampleProperty { get; set; }
    public int? OptionalProperty { get; set; }
}
```
The documentation should be structured as follows:
```csharp
/// <summary>
/// Represents an example interface with properties.
/// </summary>
public interface Example {
    /// <summary>
    /// A required property of type string.
    /// </summary>
    public string ExampleProperty { get; set; }

    /// <summary>
    /// An optional property of type int.
    /// </summary>
    public int? OptionalProperty { get; set; }
}
```

All the documentation always has to be in english, even if the code is in another language or the prompt is in another language.