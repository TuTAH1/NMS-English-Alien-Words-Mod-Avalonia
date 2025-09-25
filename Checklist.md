# Interface
- [0] Replace "Create" button text with "Update MBINC and create" if selected MBIN compiler version is not installed
- [x] Make "Create" button disabled if MBIN compiler version is not selected
- [x] Add settings window
- [x] Add classes and styles for different AvailabilityStatus
- [x] Add settings categories
- [x] Add progressbar/spinner when downloading MBINC
- [ ] Validate game path

# Functionality
- [x] Download MBIN compiler when clicking Create
- [x] Check for existing MBIN compiler before downloading
- [ ] Settings save
- [ ] Settings reset (create default settings on start)
- [ ] Unpack specific game BIN file using HGPAK tool
	- [ ] Look for MetadataEtc.pak
	- [ ] extract `LANGUAGE\/NMS_(LOC|UPDATE)\d{1,2}_ENGLISH\.BIN` (regex)
	- [ ] Unpack only needed files
- [ ] Search for words in English text files, 

- [ ] Add editable languages list – txt file?
	- [ ] language *class*, _fields_: `filename` /*is needed?*/ `languagename?`

# On release
- [ ]