import os

def rename_files_in_directory(directory, prefix='', suffix='', replace_from='', replace_to=''):
    try:
        # List all files in the directory
        for filename in os.listdir(directory):
            old_path = os.path.join(directory, filename)
            
            # Ensure it's a file (not a directory)
            if os.path.isfile(old_path):
                name, ext = os.path.splitext(filename)
                
                # Modify filename
                new_name = f"{prefix}{name.replace(replace_from, replace_to)}{suffix}{ext}"
                new_path = os.path.join(directory, new_name)
                
                # Rename file
                os.rename(old_path, new_path)
                print(f"Renamed: {filename} -> {new_name}")
    except Exception as e:
        print(f"Error: {e}")

directory_path = "./"  
rename_files_in_directory(directory_path, prefix="", replace_from="_shadow_", replace_to="_")