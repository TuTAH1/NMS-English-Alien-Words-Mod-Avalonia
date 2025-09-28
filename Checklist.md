# Interface
- [0] Replace "Create" button text with "Update MBINC and create" if selected MBIN compiler version is not installed
- [x] Make "Create" button disabled if MBIN compiler version is not selected
- [x] Add settings window
- [x] Add classes and styles for different AvailabilityStatus
- [x] Add settings categories
- [x] Add progressbar/spinner when downloading MBINC
- [x] Validate game path
- [x] Add console output
	- [x] In markdown style
	- [x] Add exception description (and recursive inner exception handling) in console
- [ ] Add remove MBIN compiler version button

# Functionality
- [x] Unpack specific game PAK file using HGPAK tool
	- [x] Look for MetadataEtc.pak
	- [x] extract `LANGUAGE\/NMS_(LOC|UPDATE)\d{1,2}_ENGLISH\.BIN` (regex)
	- [x] Unpack only needed files
		- [x] get filelist json
		- [x] make new filelist json using regex
		- [x] unback bin using new filelist json
	- [ ] Unpack all .mbin files with mbinCompiler
- [ ] Search for words in English text files, 

- [x] Add editable languages list
- [x] Download MBIN compiler when clicking Create
- [x] Check for existing MBIN compiler before downloading
- [ ] Settings save
- [ ] Settings reset (create default settings on start)
- [ ] Add cancel button and realize cancellation token

# On release
- [ ]

# Notes
## MBIN Compiler

### Highlights

-q // Quiet mode - no console output. DO NOT USE untill finish debugging

### -help output

Usage:

	MBINCompiler help [<Option>...]
	MBINCompiler version [<Option>...] [<File>]
	MBINCompiler register [<Option>...]
	MBINCompiler [convert] [<Option>...] <Path> [<Path>...]


Modes:

  help              Show this help info.
  version           Show version info.
  convert           Convert files between MBIN and MXML formats.
  register          Add MBINCompiler to your systems PATH variable.


General Options:
  -q, --quiet               Do not display any console messages.
							(Except requested help or version info.)
							Do not wait for key press.

  -Q, --nolog               Do not generate a log for the conversion.


version [<Option>...] [<File>]

	If no valid <File> is specified, the version for this exe is displayed.
	If <File> is an MBIN, the version that the MBIN was compiled with will be displayed.

	If -q or --quiet is used, a compact version string will be displayed with no decoration.


[convert] [<Option>...] <Path> [<Path>...]

	This mode is the default. The convert keyword is optional.
	For each <Path>, convert all files between MBIN and MXML formats.

convert Options:
  -y, --overwrite           Always overwrite files if they already exist.

  -n, --keep                Never overwrite files if they already exist.

  -f, --force               Skip files with errors and continue processing.
							Do not pause for errors.
							(Any errors will be written to MBINCompiler.log)

  --no-version              Hide version info in MXML header.

  -d<Directory>, --output-dir=<Directory>
							Specify the directory where files will be written to.
							If this option is used, only one input <Path> can be specified.

  -i<Type>, --input-format=<Type>
							Specify the type of input files to be converted from.
							<Type> can be either MBIN or MXML.

  -o<Type>, --output-format=<Type>
							Specify the type of output files to be converted to.
							<Type> can be either MBIN, MXML or EXML.
							Note that MBINCompiler will not accept EXML files as input.

  --include=<Glob Pattern>[;<Glob Pattern>...]
							Filter all files to include only those that match the glob patterns. A glob pattern is a
							filepath with wildcards.The * and ? wildcard characters can be used.
							Multiple glob patterns are separated by a semicolon.
							The default is --include="*.MBIN;*.MBIN.PC;*.MXML" (all).
							The --include filter is applied before --exclude.

  --exclude=<Glob Pattern>[;<Glob Pattern>...]
							Filter all files to exclude any that match the glob patterns. A glob pattern is a filepath
							with wildcards.The * and ? wildcard characters can be used.
							Multiple glob patterns are separated by a semicolon.
							The default is --exclude="" (nothing).
							The --exclude filter is applied after --include.

  --stream                  Enable sending MXML to Console.

## HGPAK tool

### Highlights
- L {path}                      list contents of pak file
- j {jsonPath} -U {pakPaths}    json file input for unpacking

### Help output

positional arguments:
  filenames             The file(s) to pack or unpack. If this is a list of pak files or a directory, then it will be assumed that the files need to be unpacked.
						If the filename is a single json file in the same format as produced by the -L flag, then it will also be unpacked as per the listed pak files and listed contents.

options:
  -h, --help            show this help message and exit
  -L, --list            Generate a list of files (filenames.json) contained within the pak file.
  -N, --nocontents      [DEPRECATED] Store the contents of a .pak in a file for recompression
  -p, --plain           Whether to output any generation informational files in a simplified format
  -C, --contents        Store the contents of a .pak in a file for recompression
  -v, --verbose         Log extra info when (un)packing archives
  --platform [{windows,mac,switch}]
						The platform to unpack the files for. Default: windows.
  -Z, --compress        Whether or not to compress the provided files.
  -O, --output OUTPUT   The directory where to place extracted files in. If not provided, falls back to a folder called 'EXTRACTED' in the current directory.
						Otherwise, the directory to send the generated output to.
  -f, --filter FILTER   A glob pattern which can be used to filter out the files which are to be extracted.
  -j, --json JSON       The path to a json file which can be used to indicate the files to be unpacked.
						The keys to the json are the paths (either relative or absolute) to the pak's that are to be extracted, and the values are the files within these pak's to extract.
						If the pak paths are relative, then a 'filename' argument MUST be passed which is the root directory of the provided pak names.
  --upper               If provided, extracted filenames will be converted to UPPERCASE.
  -U, --unpack          Unpack the files from the provided pak files.
  -P, --pack            Pack the provided files into a pak file.
  -R, --repack          Repack the files for a given vanilla pak name.
  -A, --amumssverbose   Outputs/restricts extra info for AMUMSS
  -HA, --hash HASH      Create HASH file of all paks content