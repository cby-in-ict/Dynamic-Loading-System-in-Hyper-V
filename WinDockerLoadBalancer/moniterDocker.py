import docker

class DockerMoniter:
    # client = docker.from_env()
    def __init__(self):
        self.client = docker.from_env()
        self.containers = self.getImageItems()
        merit_list = ['usage', 'limit', 'mem_use_percent', 'total_cpu_usage', 'system_cpu_usage', 'cpu_usage_percent',
                      'rx_bytes', 'tx_bytes']

    # return a docker list
    def getImageItems(self):
        ContainerList = self.client.containers.list(all=True)
        for Container in ContainerList:
            print(Container.name)
            print(Container.id)
            print(Container.status)
        return ContainerList

    # return list contains docker containers
    def getContainerStats(self, container, isDecode = False, isStream = False):
        return container.stats()

    # return a container related all performance counter
    def getContainerPerfDict(self, container, merit):
        stats = self.getContainerStats(container)
        PerdDict = {}
        if merit == 'mem_use_percent':


        elif merit == 'system_cpu_usage':
        elif merit == 'total_cpu_usage':


    def UpdateContainer(self, container, blkio_weight = None, cpu_period = None,
                        cpu_quota = None, cpu_shares = None, cpuset_cpus = None,
                        cpuset_mems = None, mem_limit = None, mem_reservation = None,
                        memswap_limit = None, kernel_memory = None):

        pass

if __name__ == "__main__":
    dockerMoniter = DockerMoniter()
    containerList = dockerMoniter.getImageItems()
    for container in containerList:
        print(container.stats(decode=True, stream=True))

