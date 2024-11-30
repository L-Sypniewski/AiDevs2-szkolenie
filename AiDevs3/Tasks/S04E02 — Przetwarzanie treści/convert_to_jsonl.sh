#!/bin/bash

txt_file="$1"
assistant_value="$2"
jsonl_file="${txt_file%.txt}.jsonl"

while IFS= read -r line; do
    echo '{"messages":[{"role":"system","content":"You are provided with research results. Decide whether the results are valid or have been manipulated."},{"role":"user","content":"'"$line"'"},{"role":"assistant","content":"'"$assistant_value"'"}]}' >>"$jsonl_file"
done <"$txt_file"
