# PdfEditApp

WPF(.NET 8) 기반의 Windows 데스크톱 PDF 편집 도구 샘플입니다. UI와 로직을 분리하고 MVVM 패턴(CommunityToolkit.Mvvm)으로 구성했습니다.

## 구성
- `PdfEditApp.sln`
- `PdfEditApp` : WPF UI
- `PdfEdit.Core` : PDF 작업 로직(qpdf.exe 호출)

## 요구 사항
- .NET 8 SDK
- qpdf.exe
- (권장) Visual Studio 2022 이상

## Visual Studio 2015에서 빌드/실행
> **주의:** Visual Studio 2015는 .NET 8 WPF 프로젝트를 직접 빌드/실행할 수 없습니다. 아래 절차는 **VS2015를 코드 편집용으로 사용**하고, 빌드/실행은 .NET 8 SDK의 `dotnet` CLI로 수행하는 방식입니다.

1. .NET 8 SDK 설치
2. VS2015로 `PdfEditApp.sln` 열어 코드 편집
3. **Developer Command Prompt for VS2015**(또는 일반 터미널)에서 다음 명령 실행
   ```bash
   dotnet build PdfEditApp.sln
   dotnet run --project PdfEditApp/PdfEditApp.csproj
   ```

> 실제 디버깅/디자이너를 사용하려면 Visual Studio 2022 이상을 권장합니다.

## qpdf 준비 방법
1. qpdf 공식 배포판 다운로드 후 설치
2. `qpdf.exe` 경로를 아래 중 하나로 지정
   - `PdfEditApp/appsettings.json`의 `Qpdf:Path` 수정
   - 앱 실행 후 UI에서 `qpdf.exe` 경로 직접 선택

## 사용 방법
1. 입력 파일 추가
2. 출력 폴더/파일명 지정
3. 작업 선택(Merge / Split / Rotate / Reorder)
4. 필요 시 옵션 설정 (Split 패턴, 회전 각도, 페이지 순서)
5. 실행 버튼 클릭

## UI 구조
- 좌측 네비게이션: 작업 선택
- 우측 패널: 입력/출력/QPDF 설정 및 작업 옵션
- 하단: 로그/진행률

## 참고
- PDF 작업은 `qpdf.exe`를 `ProcessStartInfo`로 실행합니다.
- 작업 취소는 `CancellationToken`으로 처리합니다.
