#!/usr/bin/basl
echo Hello from subshell!

if [[ "$USER" == "root" ]]; then
   echo You are root
else
   echo You are NOT root
fi

echo End of if-else statement