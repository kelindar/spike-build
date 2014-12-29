﻿// ResourceStrings.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;

namespace Spike.Build.Minifiers
{
    /// <summary>
    /// Represents a resource strings for the minifier.
    /// </summary>
    public class MinifierResourceStrings
    {
        /// <summary>
        /// The name of the collection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name value pairs of the resource strings.
        /// </summary>
        public IDictionary<string, string> NameValuePairs { get; private set; }

        /// <summary>
        /// Gets the value of a resource string.
        /// </summary>
        /// <param name="name">The name of the resource string.</param>
        /// <returns>The value.</returns>
        public string this[string name]
        {
            get
            {
                string propertyValue;
                if (!this.NameValuePairs.TryGetValue(name, out propertyValue))
                {
                    // couldn't find the property -- make sure we return null
                    propertyValue = null;
                }

                return propertyValue;
            }
            set
            {
                this.NameValuePairs[name] = value;
            }
        }

        /// <summary>
        /// Gets the count of key value pairs in this collection.
        /// </summary>
        public int Count
        {
            get { return this.NameValuePairs.Count; }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MinifierResourceStrings"/>.
        /// </summary>
        /// <param name="enumerator">The dictionary to enumerate for values.</param>
        public MinifierResourceStrings(IDictionaryEnumerator enumerator)
        {
            this.NameValuePairs = new Dictionary<string, string>();

            if (enumerator != null)
            {
                // use the IDictionaryEnumerator to add properties to the collection
                while (enumerator.MoveNext())
                {
                    // get the property name
                    string propertyName = enumerator.Key.ToString();

                    // set the name/value in the resource object
                    this.NameValuePairs[propertyName] = enumerator.Value.ToString();
                }
            }
        }
    }
}