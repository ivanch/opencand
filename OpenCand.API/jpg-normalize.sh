#!/bin/bash
cd ./fotos_cand
COUNT=0
shopt -s nocaseglob

# Loop through all folders
for dir in */; do
  # Change into the directory
  cd "$dir" || continue
  
  # Loop over every “.jpeg” (or “.JPEG”):
  for f in *.jpeg; do
    # “${f%.[jJ][pP][eE][gG]}” strips off the .jpeg/.JPEG suffix
    base="${f%.[jJ][pP][eE][gG]}"
    newfile="${base}.jpg"

    # If there’s already a .jpg with the same “base,” decide what to do:
    if [ -e "$newfile" ]; then
      echo "Skipping $f → $newfile (target exists)"
      # you could `rm "$f"` or move it to a backup folder here if you prefer
    else
      mv -v "$f" "$newfile"
    fi
  done
  
  # Change back to the parent directory
  cd ..
done

shopt -u nocaseglob

# Print a message indicating completion
echo "Normalization complete. Processed $COUNT files."