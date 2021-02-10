namespace FileFS.Client.Tests
{
    public class FileFsClientTests
    {
        public void Create_WithValidParameters_ShouldCreateFile() {}

        public void Create_WithInvalidFileName_ShouldThrowException() {}

        public void Create_WhenFileAlreadyExists_ShouldThrowException() {}

        public void Create_WithEmptyData_ShouldThrowException() {}


        public void CreateStreamed_WithValidParameters_ShouldCreateFile() {}

        public void CreateStreamed_WithInvalidFileName_ShouldThrowException() {}

        public void CreateStreamed_WhenFileAlreadyExists_ShouldThrowException() {}

        public void CreateStreamed_WithEmptyData_ShouldThrowException() {}


        public void Update_WithValidParameters_ShouldCreateFile() {}

        public void Update_WithInvalidFileName_ShouldThrowException() {}

        public void Update_WhenFileNotExists_ShouldThrowException() {}

        public void Update_WithEmptyData_ShouldThrowException() {}


        public void UpdateStreamed_WithValidParameters_ShouldCreateFile() {}

        public void UpdateStreamed_WithInvalidFileName_ShouldThrowException() {}

        public void UpdateStreamed_WhenFileNotExists_ShouldThrowException() {}

        public void UpdateStreamed_WithEmptyData_ShouldThrowException() {}


        public void Read_WithValidParameters_ShouldReadFile() {}

        public void Read_WithInvalidFileName_ShouldThrowException() {}


        public void ReadStreamed_WithValidParameters_ShouldReadFile() {}

        public void ReadStreamed_WithInvalidFileName_ShouldThrowException() {}

        public void ReadStreamed_WithDestinationStreamNull_ShouldThrowException() {}


        public void Rename_WithValidParameters_ShouldRenameFile() {}

        public void Rename_WithOneOfNameIsInvalid_ShouldThrowException() {}


        public void Delete_WithValidParameters_ShouldDeleteFile() {}

        public void Delete_WithInvalidFileName_ShouldThrowException() {}

        public void Delete_WhenFileNotExists_ShouldThrowException() {}


        public void Import_WithValidParameters_ShouldImportFile() {}

        public void Import_WithInvalidFileName_ShouldThrowException() {}

        public void Import_WhenFileAlreadyExists_ShouldThrowException() {}

        public void Import_WhenExternalFileNotExists_ShouldThrowException() {}


        public void Export_WithValidParameters_ShouldExportFile() {}

        public void Export_WithInvalidFileName_ShouldThrowException() {}

        public void Export_WhenFileNotExists_ShouldThrowException() {}

        public void Export_WhenExternalFileAlreadyExists_ShouldThrowException() {}


        public void Exists_WithValidParametersAndItemExists_ShouldReturnTrue() {}

        public void Exists_WithValidParametersAndItemNotExists_ShouldReturnFalse() {}

        public void Exists_WithInvalidFileName_ShouldThrowException() {}


        public void ListFiles_WhenThereAreFiles_ShouldReturnAllFiles() {}

        public void ListFiles_WhenThereAreNoFiles_ShouldReturnEmptyCollection() {}


        public void ForceOptimize_ShouldCallOptimizer() {}
    }
}