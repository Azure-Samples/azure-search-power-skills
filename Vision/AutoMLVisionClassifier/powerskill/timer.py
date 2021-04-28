import time
import logging

def timefunc(f):
    def f_timer(*args, **kwargs):
        start = time.time()
        result = f(*args, **kwargs)
        end = time.time()
        logging.info(f" Function {f.__name__} took {end - start} to run")
        return result
    return f_timer