import subprocess
import os
import shutil
import zipfile
from pathlib import Path

# Configuration
project_path = "."  # path to your .NET project or solution file
configuration = "Release"
output_base_dir = "../publish_output"
compressed_output_dir = os.path.join(output_base_dir, "compressed_builds")

# List of target runtimes (RIDs) to publish for
runtimes = ["win-x64", "linux-x64", "osx-x64"]

# Whether to publish self-contained (includes .NET runtime)
self_contained = True


def run_publish(runtime):
    print(f"Publishing for runtime: {runtime} (self-contained={self_contained})")

    # Build the dotnet publish command
    cmd = [
        "dotnet",
        "publish",
        project_path,
        "-c",
        configuration,
        "-r",
        runtime,
        "--output",
        os.path.join(output_base_dir, runtime),
    ]

    if self_contained:
        cmd.append("--self-contained")
    else:
        cmd.append("--no-self-contained")

    # Run the publish command
    result = subprocess.run(cmd, capture_output=True, text=True)

    if result.returncode != 0:
        print(f"Error publishing for {runtime}:\n{result.stderr}")
        return False
    else:
        print(f"Published successfully for {runtime}")
        return True


def compress_output(runtime):
    """Compress the published output for a specific runtime"""
    runtime_dir = os.path.join(output_base_dir, runtime)

    if not os.path.exists(runtime_dir):
        print(f"Output directory {runtime_dir} does not exist, skipping compression")
        return False

    # Create compressed output directory if it doesn't exist
    print(f"Creating compressed output directory: {compressed_output_dir}")
    os.makedirs(compressed_output_dir, exist_ok=True)

    # Get project name for the zip file (or use a default)
    project_name = get_project_name()
    zip_filename = f"{project_name}-{runtime}.zip"
    zip_path = os.path.join(compressed_output_dir, zip_filename)

    print(f"Compressing {runtime} build to {zip_filename}...")
    print(f"Source directory: {runtime_dir}")
    print(f"Target zip file: {zip_path}")

    try:
        with zipfile.ZipFile(
            zip_path, "w", zipfile.ZIP_DEFLATED, compresslevel=9
        ) as zipf:
            file_count = 0
            for root, dirs, files in os.walk(runtime_dir):
                for file in files:
                    file_path = os.path.join(root, file)
                    # Create relative path for the zip
                    arcname = os.path.relpath(file_path, runtime_dir)
                    zipf.write(file_path, arcname)
                    file_count += 1

            print(f"Added {file_count} files to zip")

        # Get compression stats
        original_size = get_directory_size(runtime_dir)
        compressed_size = os.path.getsize(zip_path)
        compression_ratio = (1 - compressed_size / original_size) * 100

        print(
            f"Compressed {runtime}: {format_size(original_size)} → {format_size(compressed_size)} "
            f"({compression_ratio:.1f}% reduction)"
        )
        return True

    except Exception as e:
        print(f"Error compressing {runtime} build: {e}")
        import traceback

        traceback.print_exc()
        return False


def get_project_name():
    """Extract project name from .csproj files or use directory name"""
    csproj_files = list(Path(project_path).glob("*.csproj"))
    if csproj_files:
        return csproj_files[0].stem
    return os.path.basename(os.path.abspath(project_path))


def get_directory_size(directory):
    """Calculate total size of directory in bytes"""
    total = 0
    for root, dirs, files in os.walk(directory):
        for file in files:
            total += os.path.getsize(os.path.join(root, file))
    return total


def format_size(bytes):
    """Format bytes to human readable format"""
    for unit in ["B", "KB", "MB", "GB"]:
        if bytes < 1024.0:
            return f"{bytes:.1f} {unit}"
        bytes /= 1024.0
    return f"{bytes:.1f} TB"


def clean_output():
    """Clean output directory but preserve structure for compressed builds"""
    if os.path.exists(output_base_dir):
        print(f"Cleaning output directory: {output_base_dir}")
        shutil.rmtree(output_base_dir)
    os.makedirs(output_base_dir, exist_ok=True)


def main():
    print("Starting .NET project publishing and compression process...")
    print(f"Project: {get_project_name()}")
    print(f"Configuration: {configuration}")
    print(f"Target runtimes: {', '.join(runtimes)}")
    print(f"Self-contained: {self_contained}")
    print("-" * 50)

    clean_output()

    successful_builds = []

    # Publish for each runtime
    for runtime in runtimes:
        success = run_publish(runtime)
        if success:
            successful_builds.append(runtime)
        else:
            print(
                f"Failed to publish for {runtime}, skipping compression for this runtime."
            )

    if not successful_builds:
        print("No successful builds to compress.")
        return

    print("\nStarting compression process...")
    print("-" * 30)

    # Compress successful builds
    compressed_count = 0
    for runtime in successful_builds:
        if compress_output(runtime):
            compressed_count += 1

    print("\nProcess completed!")
    print(f"Successfully published: {len(successful_builds)}/{len(runtimes)} runtimes")
    print(
        f"Successfully compressed: {compressed_count}/{len(successful_builds)} builds"
    )

    if compressed_count > 0:
        print(
            f"\nCompressed builds available in: {os.path.abspath(compressed_output_dir)}"
        )


if __name__ == "__main__":
    main()
