using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build
{
    /// <summary>
    /// Represents a template for a file.
    /// </summary>
    internal interface ITemplate
    {
        /// <summary>
        /// Gets or sets the target to build
        /// </summary>
        string Target { get; set; }

        /// <summary>
        /// Gets or sets the model to build for.
        /// </summary>
        Model Model { get; set; }

        /// <summary>
        /// Gets or sets the target operation.
        /// </summary>
        Operation TargetOperation { get; set; }

        /// <summary>
        /// Gets or sets the target type.
        /// </summary>
        CustomType TargetType { get; set;  }

        /// <summary>
        /// Transforms the T4 Template and returns the compiled text.
        /// </summary>
        /// <returns>The text of the template.</returns>
        string TransformText();

        /// <summary>
        /// Clears the output of the template.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Contract that represents a builder.
    /// </summary>
    internal interface IBuilder
    {
        /// <summary>
        /// Build the model of the specified type.
        /// </summary>
        /// <param name="model">The model to build.</param>
        /// <param name="output">The output type.</param>
        /// <param name="format">The format to apply.</param>
        void Build(Model model, string output = null, string format = null);

    }

    /// <summary>
    /// Represents a base builder for various client builders, containing helper methods.
    /// </summary>
    internal abstract class BuilderBase : IBuilder
    {
        /// <summary>
        /// Gets the extension for this builder.
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Gets whether the build should apply identation.
        /// </summary>
        public virtual bool Indent
        {
            get { return false; }
        }

        /// <summary>
        /// Build the model of the specified type.
        /// </summary>
        /// <param name="model">The model to build.</param>
        /// <param name="output">The output type.</param>
        /// <param name="format">The format to apply.</param>
        public abstract void Build(Model model, string output, string format);

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="target">The target name.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildTarget(string target, string outputDirectory, ITemplate template)
        {
            template.Target = target;
            File.WriteAllText(
                Path.Combine(outputDirectory, target + this.Extension),
                template.AsText(this.Indent)
                );
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="operation">The target operation.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildOperation(Operation operation, string outputDirectory, ITemplate template)
        {
            template.TargetOperation = operation;
            File.WriteAllText(
                Path.Combine(outputDirectory, operation.Name + this.Extension),
                template.AsText(this.Indent)
                );
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildType(CustomType type, string outputDirectory, ITemplate template)
        {
            template.TargetType = type;
            File.WriteAllText(
                Path.Combine(outputDirectory, type.Name + this.Extension),
                template.AsText(this.Indent)
                );
            template.Clear();
        }
    }
}
