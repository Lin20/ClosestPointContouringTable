# ClosestPointContouringTable
Terrible code for generating the closest point contouring table.

There's a paper out there titled, "Direct Contouring of Implicit Closest Point Surfaces" by S. Auer and R. Westermann. They describe an algorithm titled "Closest Point Contouring", which operates using a 256-entry LUT similar to Marching Cubes. It describes cases for adding quads when a face's vertices are all marked as inside, as well as 8 cases shown here:

![8 Described Cases](https://github.com/Lin20/ClosestPointContouringTable/raw/master/cpc_cases.PNG)

Oddly enough, there are more unique cases, of which this code observes but ignores per the paper. Whether or not that's correct has yet TBD. As of this writing, the table is untested.

Auer, Stefan, and Rüdiger Westermann. "Direct Contouring of Implicit Closest Point Surfaces." Eurographics (Short Papers). 2013.
