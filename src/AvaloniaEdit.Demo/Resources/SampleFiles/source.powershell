<#
.SYNOPSIS
  A simple script that solves the standard FizzBuzz interview problem.

.DESCRIPTION
  A simple script that solves the FizzBuzz interview problem to illustrate flow
control within a PowerShell script.

.PARAMETER Min
  The minimium value to start counting at (inclusive). Defaults to 1.

.PARAMETER Max
  The maximum value to count to (inclusive). Defaults to 100.

.EXAMPLE
  .\FizzBuzz.ps1
1
2
Fizz
4
Buzz
Fizz
...

.EXAMPLE
  .\FizzBuzz.ps1 -Min 0 -Max 75
FizzBuzz
1
2
Fizz
4
...

.NOTES
  For reference, here's a copy of the FizzBuzz problem that this script solves,
the only difference between the requested solution and this script is the script
will let you determine the minimum and maximum values if you desire:

"Write a program that prints the numbers 1 to 100. However, for multiples of
three, print "Fizz" instead of the number. Meanwhile, for multiples of five,
print "Buzz" instead of the number. For numbers which are multiples of both,
print "FizzBuzz"
#>
[CmdletBinding()]
param (
  [Parameter(Mandatory = $false, Position = 0)]
  $Min = 1,

  [Parameter(Mandatory = $false, Position = 1)]
  $Max = 100
)

for ($X = $Min; $X -le $Max; $X++) {
  $Output = ""

  if ($X % 3 -eq 0) { $Output += "Fizz" }
  if ($X % 5 -eq 0) { $Output += "Buzz" }
  if ($Output -eq "") { $Output = $X }

  Write-Output $Output
}
