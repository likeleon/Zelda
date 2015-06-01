# Zelda

physfs를 빌드해서 사용하고 싶다면,
physfs/windows.c의 unicodeToUtf8Heap 함수에서 PHYSFS_utf8FromUcs2 대신 WideCharToMultiByte 사용하도록 변경해야 한글이 동작
