# PDF Editor (Visual C++ 콘솔)

`qpdf` 커맨드라인 도구를 감싸서 PDF 병합, 분할, 회전, 페이지 순서 변경을 수행하는 간단한 Visual C++ 콘솔 프로그램입니다.

## 요구 사항

- Visual Studio 2022 (C++17)
- `qpdf` 설치 및 PATH 등록
  - Windows: [qpdf 공식 릴리스](https://github.com/qpdf/qpdf/releases)에서 바이너리를 내려받아 PATH에 추가하세요.

## 빌드 방법

```bash
cmake -S . -B build
cmake --build build --config Release
```

## 사용 방법

```bash
pdf_edit merge -o output.pdf input1.pdf input2.pdf ...
pdf_edit split -o output_dir input.pdf
pdf_edit rotate -o output.pdf -r <90|180|270> input.pdf
pdf_edit move -o output.pdf -p <order> input.pdf
```

### 예시

```bash
pdf_edit merge -o merged.pdf a.pdf b.pdf
pdf_edit split -o out_pages book.pdf
pdf_edit rotate -o rotated.pdf -r 90 report.pdf
pdf_edit move -o reordered.pdf -p 3,1,2 report.pdf
```

## 참고

- `split` 명령은 `output_dir/page-1.pdf`, `output_dir/page-2.pdf` 형태로 페이지를 분할합니다.
- `move` 명령의 `-p`는 새 페이지 순서를 의미합니다. 예: `3,1,2`는 3페이지를 첫 페이지로 이동합니다.
