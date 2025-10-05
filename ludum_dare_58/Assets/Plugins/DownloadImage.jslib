mergeInto(LibraryManager.library, {
  DownloadImage: function (base64DataPtr, filenamePtr) {
    var base64Data = UTF8ToString(base64DataPtr);
    var filename = UTF8ToString(filenamePtr);
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:image/png;base64," + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
});
