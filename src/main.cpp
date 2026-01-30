#include <cstdlib>
#include <filesystem>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

namespace fs = std::filesystem;

struct CommandResult {
  int exit_code;
};

CommandResult RunCommand(const std::string &command) {
  std::cout << "Running: " << command << "\n";
  int code = std::system(command.c_str());
  if (code == -1) {
    return {-1};
  }
  return {code};
}

bool EnsureQpdfAvailable() {
  auto result = RunCommand("qpdf --version");
  if (result.exit_code != 0) {
    std::cerr << "Failed to run qpdf. Ensure qpdf is installed and on PATH.\n";
    return false;
  }
  return true;
}

std::string JoinArgs(const std::vector<std::string> &args, const std::string &sep) {
  std::ostringstream oss;
  for (size_t i = 0; i < args.size(); ++i) {
    if (i > 0) {
      oss << sep;
    }
    oss << args[i];
  }
  return oss.str();
}

void PrintUsage() {
  std::cout << "PDF Editor (qpdf wrapper)\n\n"
            << "Usage:\n"
            << "  pdf_edit merge -o output.pdf input1.pdf input2.pdf ...\n"
            << "  pdf_edit split -o output_dir input.pdf\n"
            << "  pdf_edit rotate -o output.pdf -r <90|180|270> input.pdf\n"
            << "  pdf_edit move -o output.pdf -p <order> input.pdf\n\n"
            << "Examples:\n"
            << "  pdf_edit merge -o merged.pdf a.pdf b.pdf\n"
            << "  pdf_edit split -o out_pages book.pdf\n"
            << "  pdf_edit rotate -o rotated.pdf -r 90 report.pdf\n"
            << "  pdf_edit move -o reordered.pdf -p 3,1,2 report.pdf\n";
}

int MergeCommand(const std::vector<std::string> &args) {
  if (args.size() < 4) {
    PrintUsage();
    return 1;
  }

  std::string output;
  std::vector<std::string> inputs;
  for (size_t i = 1; i < args.size(); ++i) {
    if (args[i] == "-o" && i + 1 < args.size()) {
      output = args[i + 1];
      ++i;
    } else {
      inputs.push_back(args[i]);
    }
  }

  if (output.empty() || inputs.size() < 2) {
    std::cerr << "merge requires -o output.pdf and at least two input PDFs.\n";
    return 1;
  }

  std::ostringstream command;
  command << "qpdf --empty --pages " << JoinArgs(inputs, " ") << " -- " << output;
  return RunCommand(command.str()).exit_code;
}

int SplitCommand(const std::vector<std::string> &args) {
  if (args.size() < 4) {
    PrintUsage();
    return 1;
  }

  std::string output_dir;
  std::string input;
  for (size_t i = 1; i < args.size(); ++i) {
    if (args[i] == "-o" && i + 1 < args.size()) {
      output_dir = args[i + 1];
      ++i;
    } else {
      input = args[i];
    }
  }

  if (output_dir.empty() || input.empty()) {
    std::cerr << "split requires -o output_dir and an input PDF.\n";
    return 1;
  }

  fs::create_directories(output_dir);
  fs::path output_pattern = fs::path(output_dir) / "page-%d.pdf";

  std::ostringstream command;
  command << "qpdf --split-pages " << input << " " << output_pattern.string();
  return RunCommand(command.str()).exit_code;
}

int RotateCommand(const std::vector<std::string> &args) {
  if (args.size() < 5) {
    PrintUsage();
    return 1;
  }

  std::string output;
  std::string input;
  std::string rotation;

  for (size_t i = 1; i < args.size(); ++i) {
    if (args[i] == "-o" && i + 1 < args.size()) {
      output = args[i + 1];
      ++i;
    } else if (args[i] == "-r" && i + 1 < args.size()) {
      rotation = args[i + 1];
      ++i;
    } else {
      input = args[i];
    }
  }

  if (output.empty() || input.empty() || rotation.empty()) {
    std::cerr << "rotate requires -o output.pdf -r <90|180|270> and an input PDF.\n";
    return 1;
  }

  if (rotation != "90" && rotation != "180" && rotation != "270") {
    std::cerr << "rotation must be one of 90, 180, 270.\n";
    return 1;
  }

  std::ostringstream command;
  command << "qpdf --rotate=+" << rotation << ":1-z " << input << " -- " << output;
  return RunCommand(command.str()).exit_code;
}

int MoveCommand(const std::vector<std::string> &args) {
  if (args.size() < 5) {
    PrintUsage();
    return 1;
  }

  std::string output;
  std::string input;
  std::string order;

  for (size_t i = 1; i < args.size(); ++i) {
    if (args[i] == "-o" && i + 1 < args.size()) {
      output = args[i + 1];
      ++i;
    } else if (args[i] == "-p" && i + 1 < args.size()) {
      order = args[i + 1];
      ++i;
    } else {
      input = args[i];
    }
  }

  if (output.empty() || input.empty() || order.empty()) {
    std::cerr << "move requires -o output.pdf -p <order> and an input PDF.\n";
    return 1;
  }

  for (char &ch : order) {
    if (ch == ',') {
      ch = ' ';
    }
  }

  std::ostringstream command;
  command << "qpdf --pages " << input << " " << order << " -- " << output;
  return RunCommand(command.str()).exit_code;
}

int main(int argc, char *argv[]) {
  if (argc < 2) {
    PrintUsage();
    return 1;
  }

  if (!EnsureQpdfAvailable()) {
    return 1;
  }

  std::vector<std::string> args(argv + 1, argv + argc);
  const std::string &command = args[0];

  if (command == "merge") {
    return MergeCommand(args);
  }
  if (command == "split") {
    return SplitCommand(args);
  }
  if (command == "rotate") {
    return RotateCommand(args);
  }
  if (command == "move") {
    return MoveCommand(args);
  }

  std::cerr << "Unknown command: " << command << "\n";
  PrintUsage();
  return 1;
}
