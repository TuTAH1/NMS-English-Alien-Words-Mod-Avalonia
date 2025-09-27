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
	- [ ] Add exception description (and recursive inner exception handling) in console

# Functionality
- [ ] Unpack specific game PAK file using HGPAK tool
	- [x] Look for MetadataEtc.pak
	- [ ] extract `LANGUAGE\/NMS_(LOC|UPDATE)\d{1,2}_ENGLISH\.BIN` (regex)
	- [ ] Unpack only needed files
		- [0] get filelist json
		- [0] make new filelist json using regex
		- [ ] unback bin using new filelist json

- [ ] Search for words in English text files, 

- [x] Add editable languages list
- [x] Download MBIN compiler when clicking Create
- [x] Check for existing MBIN compiler before downloading
- [ ] Settings save
- [ ] Settings reset (create default settings on start)

# On release
- [ ]