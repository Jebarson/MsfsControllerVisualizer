$xamlPath = "Assets\Controllers\HoneycombAlpha.xaml"
$content = Get-Content $xamlPath -Raw

# Replace ConverterParameter=N with ConverterParameter=Joystick_Button_N for buttons 1-35
for ($i = 1; $i -le 35; $i++) {
    # Match the exact pattern with quotes
    $content = $content -replace "ConverterParameter=$i`"", "ConverterParameter=Joystick_Button_$i`""
}

# Write back to file
Set-Content $xamlPath -Value $content -NoNewline
Write-Output "Successfully updated XAML file with Joystick_Button_X format for all parameters"
