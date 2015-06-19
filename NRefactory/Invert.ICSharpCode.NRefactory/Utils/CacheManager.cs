﻿// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Invert.ICSharpCode.NRefactory.Utils
{
	/// <summary>
	/// Allows caching values for a specific compilation.
	/// A CacheManager consists of a for shared instances (shared among all threads working with that resolve context).
	/// </summary>
	/// <remarks>This class is thread-safe</remarks>
	public sealed class CacheManager
	{
		readonly Dictionary<object, object> sharedDict = new Dictionary<object, object>(ReferenceComparer.Instance);
		// There used to be a thread-local dictionary here, but I removed it as it was causing memory
		// leaks in some use cases.
		
		public object GetShared(object key)
		{
			object value;
			sharedDict.TryGetValue(key, out value);
			return value;
		}
		
		public object GetOrAddShared(object key, Func<object, object> valueFactory)
		{
		    if (!sharedDict.ContainsKey(key))
		    {
		        sharedDict.Add(key,valueFactory);
		    }
		    return sharedDict[key];
			//return sharedDict.GetOrAdd(key, valueFactory);
		}
		
		public object GetOrAddShared(object key, object value)
		{
            if (!sharedDict.ContainsKey(key))
            {
                sharedDict.Add(key, value);
            }
            return sharedDict[key];
		}
		
		public void SetShared(object key, object value)
		{
			sharedDict[key] = value;
		}
	}
}