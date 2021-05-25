from selenium import webdriver
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities

import time

class TestPerformance:
    def test_performance(self, url_list):
        desired_capabilities = DesiredCapabilities.CHROME
        desired_capabilities["pageLoadStrategy"] = "eager"

        chrome_options = webdriver.ChromeOptions()
        #chrome_options.add_experimental_option("prefs", prefs)
        chrome_options.add_argument("--ignore-certificate-error")
        chrome_options.add_argument("--ignore-ssl-errors")
        chrome_options.add_argument('--disable-gpu')
        caps = webdriver.DesiredCapabilities.CHROME.copy()
        caps['acceptInsecureCerts'] = True
        caps['acceptSslCerts'] = True
        
        driver = webdriver.Chrome(options=chrome_options, desired_capabilities=caps)

        blank = "about:blank"
        for url in url_list:
            for i in range(10):
                try:
                    #driver.findElement(By.cssSelector("body")).sendKeys(Keys.CONTROL +"t");
                    driver.execute_script("window.open('" + blank +"')")
                   
                    all_handles = driver.window_handles   #获取到当前所有的句柄,所有的句柄存放在列表当中
                    '''获取非最初打开页面的句柄'''
                    driver.switch_to_window(all_handles[-1])
                    driver.get(url)
                except Exception:
                    continue

# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    testPerf = TestPerformance()
    time_start = time.time()
    urllist = ["https://www.taobao.com", "https://www.sina.com.cn", "https://www.cctv.com", "http://xinhuanet.com", "https://www.sohu.com"]
    testPerf.test_performance(url_list=urllist)
    time_end = time.time()
    print('[+] total cost', time_end-time_start)
    input()
    try:
        with open(".\\time.txt", "a+", encoding="utf-8", errors="ignore") as f:
            f.writelines(str(time_end-time_start)+"\n")
    except Exception as exp :
        print(exp.__str__())
