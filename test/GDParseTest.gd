extends Node

## @label Custom label!
## this is a tooltip line! (default)
export(int) var someInt := 1

## custom tooltip again <3
## @label Noice
## @range -4, 5, 2, false, clamp_both
export(int) var anotherInt := 5

## a cool float <3
## @label What a float
## @range -1, 1, 0.1, false
export(float) var testFloat := 0.1
