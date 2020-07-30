
############################################################################################
#Permission is hereby granted, free of charge, to any person obtaining a copy of
#this software and associated documentation files (the "Software"), to deal in
#the Software without restriction, including without limitation the rights to
#use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
#of the Software, and to permit persons to whom the Software is furnished to do
#so, subject to the following conditions:

#The above copyright notice and this permission notice shall be included in all
#copies or substantial portions of the Software.

#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
#SOFTWARE.
###########################################################################################

from statistics import mean


class TextBlock():
  """
    A class to represent a block of text

    ...

    Attributes
    ----------
    text : str
        text in block
    boundingbox : str[]
        bounding box for text [top left x, top left y, top right x, top right y, bottom right x, bottom right y, bottom left x, bottom left y]
    topleft_x : int
        top left x possition
    topleft_y : int
        top left y possition
    topright_x : int
        top right x possition
    topright_y : int
        top right y possition
    bottomright_x : int
        bottom right x possition
    bottomright_y : int
        bottom right y possition
    bottomleft_x : int
        bottom left x possition
    bottomleft_y : int
        bottom left y possition
    x: int
      top left x
    y: int
      top left y
    w: int
      width of bounding box
    h: int
      height of bounding box
    center_x: int
      center line of bounding box

    Methods
    -------
    copy():
        Return a deep copy of TextBlock.
    def merge(other):
      Merge self with other TextBlock
  """
  
  def __init__(self,text, boundingbox):
    self.text = text
    self.boundingbox = boundingbox
    self.topleft_x = int(boundingbox[0]['x'])
    self.topleft_y = int(boundingbox[0]['y'])
    self.topright_x = int(boundingbox[1]['x'])
    self.topright_y = int(boundingbox[1]['y'])
    self.bottomright_x = int(boundingbox[2]['x'])
    self.bottomright_y = int(boundingbox[2]['y'])
    self.bottomleft_x = int(boundingbox[3]['x'])
    self.bottomleft_y = int(boundingbox[3]['y'])
    self.x = self.topleft_x 
    self.y = self.topleft_y
    self.w = abs(self.topright_x - self.topleft_x)
    self.h = abs(self.bottomleft_y-self.topleft_y)
    self.center_x = mean([self.topright_x,self.topleft_x])
    
  def copy(self):
    """Return a deep copy of TextBlock

    Returns:
       Copy TextBlock (TextBlock): New Copied TextBlock
    """
    try:
      bounds = [x for x in self.boundingbox]
      return TextBlock(self.text,bounds)
    except Exception:
      raise Exception("Unable to copy textbox")
  
  def merge(self,other):
    """
    Merge self with other TextBlock

    Parameters::
      a(TextBlock): other -- TextBlock to merge with self

    Returns:
       merged TextBlock (TextBlock): True when TextBlocks intersect, otherwise False
    """

    bounds = []
    bounds.append({"x":min(self.topleft_x,other.topleft_x),"y":min(self.topleft_y,other.topleft_y)})
    bounds.append({"x":max(self.topright_x,other.topright_x),"y":min(self.topright_y,other.topright_y)})
    bounds.append({"x":max(self.bottomright_x,other.bottomright_x),"y":max(self.bottomright_y,other.bottomright_y)})
    bounds.append({"x":min(self.bottomleft_x,other.bottomleft_x),"y":max(self.bottomleft_y,other.bottomleft_y)})

    
    ## Merge text top to bottom
    if(self.y <= other.y):
      txt = self.text + " " + other.text
    else:
      txt = other.text + " " + self.text
      
    
    return TextBlock(txt,bounds)
    

  def __iter__(self):
    return iter(self.topleft_y)
  
  def intersect(self,other):
    """
    Determine if the TextBlocks intersect

    Parameters::
      a(TextBlock): other -- Second TextBlock to merge with self

    Returns:
       intersect (boolean): True when TextBlocks intersect, otherwise False
    """

    x1 = max(min(self.topleft_x, self.bottomright_x), min(other.topleft_x, other.bottomright_x))
    y1 = max(min(self.topleft_y,self.bottomright_y), min(other.topleft_y,other.bottomright_y))
    x2 = min(max(self.topleft_x, self.bottomright_x), max(other.topleft_x, other.bottomright_x))
    y2 = min(max(self.topleft_y,self.bottomright_y), max(other.topleft_y,other.bottomright_y))
    return (x1<x2 and y1<y2)
  
  def dist(self,other):
    """
    Distance between two TextBlocks bounds

    Parameters::
      a(TextBlock): other -- TextBlocks to measure from self

    Returns:
       distance (int): Absolute distance
    """

    if abs(self.y - other.y) <= ((self.y + self.h) + other.h):
      dy = 0
    else:
      dy = abs(self.y - other.y) - (self.h + other.h)
   
    if abs(self.x - other.x) <= (self.w + other.w):
        dx = 0
    else:
        dx = abs(self.x - other.x) - (self.w + other.w)
    

    return dx + dy
  
  def dist_y(self,other):
    """
    Distance between TextBlocks y coordinates using bottom of self with top of other

    Parameters::
      a(TextBlock): other -- TextBlocks to measure from self

    Returns:
       distance (int): Absolute y distance
    """
    #overlaps in x or y:
    #if abs(self.y - other.y) <= (self.h + other.h):
    #  dy = 0
    #else:
    #  dy = abs(self.y - other.y) - (self.h + other.h)
    
    return abs(self.bottomleft_y - other.y)

  def dist_mean_x(self,other):
    """
    Distance between TextBlocks average x (center line)

    Parameters::
      a(TextBlock): other -- TextBlocks to measure from self

    Returns:
       distance (int): Absolute mean x distance
    """
    #overlaps in x or y:
    #if abs(self.x - other.x) <= (self.w + other.w):
    #    dx = 0;
    #else:
    #    dx = abs(self.x - other.x - (self.w + other.w))
    
    return abs(self.center_x - other.center_x)
  
  def dist_left_x(self,other):
    """
    Distance between TextBlocks left x (left aligned)

    Parameters:
      a(TextBlock): other -- TextBlock to measure from self

    Returns:
       distance (int): Absolute left x distance
    """

    return abs(self.x - other.x)
  
  def dist_right_x(self,other):
    """
    Distance between TextBlocks right x (right aligned)

    Parameters:
      a(TextBlock): other -- TextBlock to measure from self

    Returns:
       distance (int): Absolute right x distance
    """
    return abs(self.topright_x - other.topright_x)
    
  def avg_y(self):
    """
    Distance between TextBlocks raverage y (vertical center)

    Parameters:
      a(TextBlock): other -- TextBlock to measure from self

    Returns:
       distance (int): Absolute mean y distance
    """
    return mean([self.topleft_y,self.bottomleft_y])