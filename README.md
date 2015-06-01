# Zelda

physfs를 빌드해서 사용하고 싶다면 physfs/platform/windows.c의 수정이 필요 (2.0.3 버전 기준).

한글이 포함된 경로를 인식하지 못하는 문제가 있다.

* unicodeToUtf8Heap 함수에서 PHYSFS_utf8FromUcs2 대신 WideCharToMultiByte 사용하도록 변경
* UTF8이 입력인 것이 확실한데, CP_ACP를 사용하여 시스템 코드 페이지 기반으로 동작하도록 되어있는 부분들을 CP_UTF8로 변경
