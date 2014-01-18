EVECacheParser - .NET 4/C# EVE Cache File Parser Library
Copyright © 2012 Jimi 'Desmont McCallock' C <jimikar@gmail.com>

EVECacheParser is an EVE Online cache/bulkdata file parser library.


==LICENSE==

EVECacheParser is distributed under GPL v2 
(see license.txt that is included with the distribution).


==REQUIREMENTS==

- Windows (XP or later).
- x86/x64 compatible processor.
- .NET Framework 4 or higher (possible support from Mono, although not tested)
- An EVE Online installation.

Notes:

- A full EVE installation is not required in every case. It is perfectly
  acceptable to have only the bulkdata and cache folders in the EVE root.

- On Windows, the location of the cache folder is automatically detected
  (in Local AppData, or EVE's root when EVE is normally run with /LUA:OFF).

  
==SECURITY WARNING==

!!! DO NOT DECODE DATA FROM UNTRUSTED SOURCES WITH THIS LIBRARY !!!
Decoding maliciously constructed or erroneous data may compromise 
your system's security and/or stability.


==DISCLAIMER==

This product does not modify in any way any file associated with the 
EVE client or writes files that change the EVE client behavior, 
therefore does not violate CCP's EVE Online EULA.

EVE Online is a registered trademark of CCP hf.


==HOW TO USE==

Every method is static and can be accessed via the Parser class.
Parser exposes the following methods:

- SetCachedFilesFolders(params string[] args)
		Method parameter: The name or names of a cache folder.
	Use to explicitly set which folders that contain cache files
	you want the parser to search.
	Default folder is "CachedMethodCalls" if none is set.
	You can either set them all at once or one by one,
	internally they get accumulated.
	e.g.
		Parser.SetCachedFilesFolders("CachedMethodCalls", "CachedObjects");
	or
		Parser.SetCachedFilesFolders("CachedMethodCalls");
        Parser.SetCachedFilesFolders("CachedObjects");

		
- SetIncludeMethodsFilter(params string[] args)
		Method parameter: The name or names of a method.
	Use to explicitely include methods of cached files.
	You can either set them all at once or one by one,
	internally they get accumulated.
	Note: A list of available methods can be found at http://wiki.eve-id.net/Cache_Resources
	e.g.
		Parser.SetIncludeMethodsFilter("GetOrders", "GetBookmarks");
	or
		Parser.SetIncludeMethodsFilter("GetOrders");
        Parser.SetIncludeMethodsFilter("GetBookmarks");


- SetExcludeMethodsFilter(params string[] args)
		Method parameter: The name or names of a method.
	Use to explicitely exclude methods of cached files.
	You can either set them all at once or one by one,
	internally they get accumulated.
	Note: A list of available methods can be found at http://wiki.eve-id.net/Cache_Resources
	e.g.
		Parser.SetExcludeMethodsFilter("GetOrders", "GetBookmarks");
	or
		Parser.SetExcludeMethodsFilter("GetOrders");
        Parser.SetExcludeMethodsFilter("GetBookmarks");

Notice here that 'SetIncludeMethodsFilter' overpowers 'SetExcludeMethodsFilter'.
There is no point in including and then excluding a method.


- GetBulkDataCachedFiles(string folderPath)
		Method parameter: The path of the 'bulkdata' folder or
	the EVE installation root folder.
		Returns: An enumeration of the cached files in the 'bulkdata' folder.
	e.g.
		FileInfo[] cachedFiles = Parser.GetBulkDataCachedFiles(@"C:\Program Files\CCP\EVE\bulkdata");
	or
		FileInfo[] cachedFiles = Parser.GetBulkDataCachedFiles(@"C:\Program Files\CCP\EVE");


- GetMachoNetCachedFiles([string folderPath = null])
		Method parameter (optional): The path of the EVE installation root folder
	if you run the EVE client with /LUA:OFF.
		Returns: An enumeration of cached files from folders specified
	in SetCachedFilesFolders and with methods specified by
	SetIncludeMethodsFilter or SetExcludeMethodsFilter.
	e.g.
		FileInfo[] cachedFiles = Parser.GetMachoNetCachedFiles();
	or
		FileInfo[] cachedFiles = Parser.GetMachoNetCachedFiles(@"C:\Program Files\CCP\EVE");


- DumpStructure(FileInfo file)
		Method parameter: A cache file as returned from method
	GetBulkDataCachedFiles or GetMachoNetCachedFiles.
		Exports a txt file revealing the structure of the data in the cache file.
	e.g.
		Parser.DumpStructure(cachedFile);


- ShowAsAscii(FileInfo file)
		Method parameter: A cache file as returned from method
	GetBulkDataCachedFiles or GetMachoNetCachedFiles.
		Displays on screen the data in the cache file in ASCII format.
	e.g.
		Parser.ShowAsAscii(cachedFile);


- GetObject(object value)
		Method parameter: A cached object from a parsed cache file
	as returned from method Parse.
		Returns: A parsed object of type object.
	Usually used when parsing cache files from CachedObjects folder.
		e.g.
			object obj = Parser.GetObject(cachedObject);

		
- Parse(FileInfo file)
		Method parameter: A cache file as returned from method
	GetBulkDataCachedFiles or GetMachoNetCachedFiles.
		Returns: The cache file's parsed data in the format of
	KeyValuePair<object, object>.
		e.g.
			KeyValuePair<object, object> result = Parser.Parse(cachedFile);

Notice here that each Key and Value have to be cast to their proper type
in order to be usable (see examples in Dumper project).

Possible types for Key are:
	String, Tuple<object>, (List<object>)Tuple<object>

Possible types for Value are:
	Dictionary<object, object>,(List<object>)Dictionary<object, object>,
	(Dictionary<object, object>)Dictionary<object, object>
	(List<object>)((Tuple<object>)Dictionary<object, object>)

