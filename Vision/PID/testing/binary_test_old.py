import requests
import json
import base64
import time
import os
from queue import Queue
from threading import Thread

class ThreadPool:
    """Pool of threads consuming tasks from a queue"""
    def __init__(self, num_threads):
        self.tasks = Queue(num_threads)
        for _ in range(num_threads):
            Worker(self.tasks)

    def add_task(self, func, *args, **kargs):
        """Add a task to the queue"""
        self.tasks.put((func, args, kargs))

    def wait_completion(self):
        """Wait for completion of all the tasks in the queue"""
        self.tasks.join()


class Worker(Thread):
    """Thread executing tasks from a given tasks queue"""
    def __init__(self, tasks):
        Thread.__init__(self)
        self.tasks = tasks
        self.daemon = True
        self.start()

    def run(self):
        while True:
            func, args, kargs = self.tasks.get()
            try:
                func(*args, **kargs)
            except Exception as e:
                print(e)
            finally:
                self.tasks.task_done()





def ocr_skill(path,i):
    request = {}
    request["values"] = []
    start_time = time.time()
    try:                
        #print("---PATH----- " + path)
        fin = open(path,"rb")

        base64_bytes = base64.b64encode(fin.read())

        base64_string = base64_bytes.decode(ENCODING)

        fin.close()

        request["values"].append({"recordId": 0,"data":{"file_data": {"type":"file","url":"null","data": base64_string }}})

    except IOError:
        print("Image file %s not found" % path)
        raise SystemExit

        
                #data = json.dumps({"values": [{"recordId": "0","data":{"file_data": {"type":"file","url":"null","data": base64_string }}}]})

    data = json.dumps(request,ensure_ascii=False)    

    headers = {'Content-Type':'application/json'}
    #'Authorization':('Bearer '+ self.key)         
    resp = requests.post(baseurl ,data, headers=headers)

    print(resp.status_code)
    if resp.status_code < 300:
        jsn = resp.json()
        print(jsn)
    print("(%s) PATH %s  --- %s seconds ---" % (str(i),path,(time.time() - start_time)))



def spin_wait(thread_array,threads):
    waiting = True
    while waiting:
        th = [t for t in thread_array if t.isAlive()]
        waiting = len(th) >= threads
        if waiting:
            time.sleep(.005)
    return th


#docker run --rm  -p 8080:80 -it jscholtes/azure_function_pid_ocr:v1
#docker build --pull   --rm -f "diagramskill\Dockerfile" --build-arg COMPUTER_VISION_URL= --build-arg COMPUTER_VISION_KEY=  -t jscholtes/azure_function_pid_ocr:v1 "diagramskill"

#baseurl = "http://localhost:8080/api/app?debug=False"
baseurl = "https://pidskill.azurewebsites.net/api/app?debug=False"

ENCODING="utf-8"

location = r"C:\Users\joscholt\Downloads\Example Drawings\Input\orig"

pool = ThreadPool(10)
i = 0

thread_array = []

start_time = time.time()

for r, d, f in os.walk(location):
    for item in f:
        #if item == "CP-000000-MEC-PID-01199-00004-001.05.VDR.05.01.pdf":
        time.sleep(1)
        path = location + "\\" + item
        print('Document %s' % str(i))
        th = Thread(target=ocr_skill, args=(path,i))
        th.setDaemon(True)
        th.start()       
        thread_array.append(th)
        thread_array = spin_wait(thread_array,5)
            
       # pool.add_task(ocr_skill(path))
        i = i + 1

spin_wait(thread_array,1)

print("Total Run--- %s seconds ---" % (time.time() - start_time))
#debug capture
#print(json.dumps(resp.json()))

#if resp.status_code != 200:

#out = open("debug.txt","w")

#out.write("----Request----------\r\n" + json.dumps(data) + "\r\n\r\n-------------Response------------\r\n " + json.dumps(jsn))

#out.close()

