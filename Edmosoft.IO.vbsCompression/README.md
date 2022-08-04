Decompression library for Microsoft VBA project files


## Usage
```
System.IO.StreamReader docReader = new System.IO.StreamReader("/path/to/Office.docm");
System.IO.Compression.ZipArchive docCompression = new System.IO.Compression.ZipArchive(docReader.BaseStream);
System.IO.Compression.ZipArchiveEntry vbaProjectArchive = docCompression.GetEntry("word/vbaProject.bin");
System.IO.Stream vbaProjectStream = new System.IO.MemoryStream();
vbaProjectArchive.Open().CopyTo(vbaProjectStream);
OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(vbaProjectStream);
OpenMcdf.CFStream module = (OpenMcdf.CFStream)cf.GetAllNamedEntries("Module1")[0];
System.IO.Stream compressedModule = new System.IO.MemoryStream(module.GetData());
compressedModule.Position = moduleOffset; //moduleOffset is read from structures in the "dir" stream from the CompoundFile, "dir" is also compressed with vbaCompression
System.IO.Stream decompressedModule = Edmosoft.IO.vbaCompression.vbaStreamReader.Decode(new Edmosoft.IO.StreamReader(dir.GetData()));
string ModuleContent = new System.IO.StreamReader(decompressedModule).ReadToEnd();
```