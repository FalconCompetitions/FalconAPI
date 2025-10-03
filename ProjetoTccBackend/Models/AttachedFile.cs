using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    /// <summary>
    /// Represents a file that is attached to an entity, including its metadata such as name, type, size, and file path.
    /// </summary>
    /// <remarks>This class is typically used to store and manage information about files associated with a
    /// specific entity,  such as uploaded documents or media files. It includes metadata properties to describe the
    /// file and its location.</remarks>
    public class AttachedFile
    {
        /// <summary>
        /// The unique identifier of the AttachedFile
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the type of the entity or object represented by this instance.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the size of the item in bytes.
        /// </summary>
        public required long size { get; set; }

        /// <summary>
        /// Gets or sets the file path associated with the operation.
        /// </summary>
        public required string FilePath { get; set; }



        /// <summary>
        /// Gets the collection of exercises associated with this instance.
        /// </summary>
        public ICollection<Exercise> Exercises { get; } = [];





    }
}
