# Make your own number textures

Select `Custom` as Style in the settings for Big Countdown and set your own texture folder !

**Requirements:**

- Images must be PNG
- Files must be names from 0.png to 9.png
- Images can be any width
- Images will top-aligned, so I recommend using the same height for all numbers

**Optional settings:**

You can add a `settings.json` file in the folder to configure the following properties:

Example JSON contents with default values:

```json
{
  "NumberNegativeMargin": 10,
  "NumberNegativeMarginMono": 10,
  "NumberBottomMargin": 0
}
```

- `NumberNegativeMargin`: margin to remove when positioning the next number
- `NumberNegativeMarginMono`: same but for monospaced mode
- `NumberBottomMargin`: margin to the number baseline in pixels (to align decimal numbers)

**Example:**

[Download example custom textures](../../Doc/custom_example.zip)
